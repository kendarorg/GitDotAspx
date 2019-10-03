using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using GitAspx.Lib;
using System.Diagnostics;
using System.IO;
using GitSharp;
using GitAspx.ViewModels;
using Bonobo.Git.Server;
using System.Text;

namespace GitAspx.Controllers
{
    public class BrowseController : Controller
    {
        readonly RepositoryService repositories;

        public BrowseController(RepositoryService repositories)
        {
            this.repositories = repositories;
        }

        [FormAuthorizeEventually(IsEventual = true)]
        public FileContentResult Binary(string project, string path = "", string file = "", bool returnAsBinary = false)
        {
            bool isAuthenticated = HttpContext.User.Identity.IsAuthenticated;
            if (isAuthenticated)
            {
                isAuthenticated = AuthService.CanRead(project, HttpContext.User.Identity.Name);
            }
            if (!AuthService.IsRepositoryPublic(project) && !isAuthenticated)
            {
                return new FileContentResult(new byte[] { }, "binary/octet-stream");
            }
            var model = new LevelViewModel();

            //Opening an existing git repository
            Lib.Repository gitasprepo = repositories.GetRepository(project);
            model.Project = project;

            var directory = gitasprepo.GitDirectory();

            var repo = new GitSharp.Repository(directory);

            if (repo == null || repo.Head == null || repo.Head.CurrentCommit == null)
            {
                return new FileContentResult(new byte[] { }, "binary/octet-stream");
            }


            var tree = repo.Head.CurrentCommit.Tree;
            model.SetBreadCrumb(path.Split(Path.DirectorySeparatorChar).ToList());

            if (model.BreadCrumb.Count > 0)
            {
                foreach (var breadCrumb in model.BreadCrumb)
                {
                    var subTree = tree.Trees.Where(a => a.Name == Path.GetFileName(breadCrumb)).FirstOrDefault();
                    if (subTree != null) tree = subTree;
                }
            }

            if (string.IsNullOrEmpty(file))
            {
                if (tree == null) throw new Exception("Path not found");
                //Now you can browse throught that tree by iterating over its child trees
                foreach (Tree subtree in tree.Trees)
                {
                    model.Directories.Add(new LevelItemViewModel(model) { Name = subtree.Path });
                }

                //Or printing the names of the files it contains
                foreach (Leaf leaf in tree.Leaves)
                {
                    model.Files.Add(new LevelItemViewModel(model) { Name = leaf.Path });
                }
            }
            else
            {
                //Or printing the names of the files it contains
                foreach (Leaf leaf in tree.Leaves)
                {
                    if (file == Path.GetFileName(leaf.Path))
                    {
                        var fcr = new FileContentResult(leaf.RawData, FileDisplayHandler.GetMimeType(file));
                        fcr.FileDownloadName = leaf.Name;
                        return fcr;
                    }
                }
            }

            return new FileContentResult(new byte[] { }, "binary/octet-stream");
        }

        [FormAuthorizeEventually(IsEventual = true)]
        public ActionResult Index(string project, string path = "", string file = "", bool returnAsBinary = false)
        {
            bool isAuthenticated = HttpContext.User.Identity.IsAuthenticated;
            if (isAuthenticated)
            {
                isAuthenticated = AuthService.CanRead(project, HttpContext.User.Identity.Name);
            }
            if (!AuthService.IsRepositoryPublic(project) && !isAuthenticated)
            {
                return Redirect("../");
            }

            var model = new LevelViewModel();

            //Opening an existing git repository
            Lib.Repository gitasprepo = repositories.GetRepository(project);
            model.Project = project;

            var directory = gitasprepo.GitDirectory();

            var repo = new GitSharp.Repository(directory);

            if (repo == null || repo.Head == null || repo.Head.CurrentCommit == null)
            {
                return View(model);
            }


            var tree = repo.Head.CurrentCommit.Tree;
            model.SetBreadCrumb(path.Split(Path.DirectorySeparatorChar).ToList());

            if (model.BreadCrumb.Count > 0)
            {
                foreach (var breadCrumb in model.BreadCrumb)
                {
                    var subTree = tree.Trees.Where(a => a.Name == Path.GetFileName(breadCrumb)).FirstOrDefault();
                    if (subTree != null) tree = subTree;
                }
            }

            if (string.IsNullOrEmpty(file))
            {
                if (tree == null) throw new Exception("Path not found");
                //Now you can browse throught that tree by iterating over its child trees
                foreach (Tree subtree in tree.Trees)
                {
                    model.Directories.Add(new LevelItemViewModel(model) { Name = subtree.Path });
                }

                //Or printing the names of the files it contains
                foreach (Leaf leaf in tree.Leaves)
                {
                    model.Files.Add(new LevelItemViewModel(model) { Name = leaf.Path });
                }
            }
            else
            {
                model.Ext = Path.GetExtension(file);
                if (!string.IsNullOrEmpty(model.Ext))
                {
                    model.Ext = model.Ext.Substring(1);
                }
                //Or printing the names of the files it contains
                foreach (Leaf leaf in tree.Leaves)
                {
                    if (file == Path.GetFileName(leaf.Path))
                    {
                        model.FileName = string.Format("Binary?project={0}&path={1}&file={2}",
                                (project),
                                (path),
                                (file));

                        if (FileDisplayHandler.IsImage(file))
                        {
                            model.ImageFile = string.Format("Binary?project={0}&path={1}&file={2}",
                                (project),
                                (path),
                                (file));
                            model.FileName = model.ImageFile;
                        }
                        else
                        {
                            model.TextFile = FillFileData(leaf);

                            if (model.TextFile == null)
                            {
                                model.BinaryFile = model.FileName;
                            }
                        }
                    }
                }
            }

            return View(model);
        }

        private string FillFileData(Leaf leaf)
        {
            var result = FileDisplayHandler.GetText(leaf.RawData);
            if (result != null)
            {
                return result;
            }
            if (FileDisplayHandler.GetBrush(leaf.Name) != "plain")
            {
                return ASCIIEncoding.ASCII.GetString(leaf.RawData);
            }
            return null;
        }
    }
}