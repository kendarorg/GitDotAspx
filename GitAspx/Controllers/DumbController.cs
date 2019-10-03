namespace GitAspx.Controllers
{
    using System;
    using System.IO;
    using System.Web.Mvc;
    using System.Web.SessionState;
    using GitAspx.Lib;
    using MultiAuthLib.ThirdParties.Bonobo;

    [SessionState(SessionStateBehavior.Disabled)]
    public class DumbController : BaseController
    {
        readonly RepositoryService repositories;

        public DumbController(RepositoryService repositories)
        {
            this.repositories = repositories;
        }

        [BasicAuthorizeEventually(IsEventual = true)]
        public ActionResult GetTextFile(string project)
        {
            bool isAuthenticated = HttpContext.User.Identity.IsAuthenticated;
            if (isAuthenticated)
            {
                isAuthenticated = AuthService.CanRead(project, HttpContext.User.Identity.Name);
            }
            if (!AuthService.IsRepositoryPublic(project) && !isAuthenticated)
            {
                return AuthService.RequireBasicAuth(HttpContext);
            }
            return WriteFile(project, "text/plain");
        }

        [BasicAuthorizeEventually(IsEventual = true)]
        public ActionResult GetInfoPacks(string project)
        {
            bool isAuthenticated = HttpContext.User.Identity.IsAuthenticated;
            if (isAuthenticated)
            {
                isAuthenticated = AuthService.CanRead(project, HttpContext.User.Identity.Name);
            }
            if (!AuthService.IsRepositoryPublic(project) && !isAuthenticated)
            {
                return AuthService.RequireBasicAuth(HttpContext);
            }
            return WriteFile(project, "text/plain; charset=utf-8");
        }

        [BasicAuthorizeEventually(IsEventual = true)]
        public ActionResult GetLooseObject(string project)
        {
            bool isAuthenticated = HttpContext.User.Identity.IsAuthenticated;
            if (isAuthenticated)
            {
                isAuthenticated = AuthService.CanRead(project, HttpContext.User.Identity.Name);
            }
            if (!AuthService.IsRepositoryPublic(project) && !isAuthenticated)
            {
                return AuthService.RequireBasicAuth(HttpContext);
            }
            return WriteFile(project, "application/x-git-loose-object");
        }

        [BasicAuthorizeEventually(IsEventual = true)]
        public ActionResult GetPackFile(string project)
        {
            bool isAuthenticated = HttpContext.User.Identity.IsAuthenticated;
            if (isAuthenticated)
            {
                isAuthenticated = AuthService.CanRead(project, HttpContext.User.Identity.Name);
            }
            if (!AuthService.IsRepositoryPublic(project) && !isAuthenticated)
            {
                return AuthService.RequireBasicAuth(HttpContext);
            }
            return WriteFile(project, "application/x-git-packed-objects");
        }

        [BasicAuthorizeEventually(IsEventual = true)]
        public ActionResult GetIdxFile(string project)
        {
            bool isAuthenticated = HttpContext.User.Identity.IsAuthenticated;
            if (isAuthenticated)
            {
                isAuthenticated = AuthService.CanRead(project, HttpContext.User.Identity.Name);
            }
            if (!AuthService.IsRepositoryPublic(project) && !isAuthenticated)
            {
                return AuthService.RequireBasicAuth(HttpContext);
            }
            return WriteFile(project, "application/x-git-packed-objects-toc");
        }

        private ActionResult WriteFile(string project, string contentType)
        {
            bool isAuthenticated = HttpContext.User.Identity.IsAuthenticated;
            if (isAuthenticated)
            {
                isAuthenticated = AuthService.CanRead(project, HttpContext.User.Identity.Name);
            }
            if (!AuthService.IsRepositoryPublic(project) && !isAuthenticated)
            {
                return AuthService.RequireBasicAuth(HttpContext);
            }

            Response.WriteNoCache();
            Response.ContentType = contentType;
            var repo = repositories.GetRepository(project);

            string path = Path.Combine(repo.GitDirectory(), GetPathToRead(project));

            if (!System.IO.File.Exists(path))
            {
                return new NotFoundResult();
            }

            Response.WriteFile(path);

            return new EmptyResult();
        }

        private string GetPathToRead(string project)
        {
            int index = Request.Url.PathAndQuery.IndexOf(project) + project.Length + 1;
            return Request.Url.PathAndQuery.Substring(index);
        }
    }
}