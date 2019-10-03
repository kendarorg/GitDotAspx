#region License

// Copyright 2010 Jeremy Skinner (http://www.jeremyskinner.co.uk)
//  
// Licensed under the Apache License, Version 2.0 (the "License"); 
// you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at 
// 
// http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software 
// distributed under the License is distributed on an "AS IS" BASIS, 
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
// See the License for the specific language governing permissions and 
// limitations under the License.
// 
// The latest version of this file can be found at http://github.com/JeremySkinner/git-dot-aspx

#endregion

namespace GitAspx.Controllers
{
    using System.Web.Mvc;
    using GitAspx.Lib;
    using GitAspx.ViewModels;
    using System.Linq;
    using System.Web;
    using System.Web.Security;
    using System.Security.Principal;

    public class DirectoryListController : Controller
    {
        readonly RepositoryService repositories;

        public DirectoryListController(RepositoryService repositories)
        {
            this.repositories = repositories;
        }

        [FormAuthorizeEventually(IsEventual = true)]
        public ActionResult Index()
        {
            var dlvm = new DirectoryListViewModel
            {
                RepositoriesDirectory = repositories.GetRepositoriesDirectory().FullName,
                Repositories = repositories.GetAllRepositories().Select(x => new RepositoryViewModel(x)).ToList()
            };

            bool isAuthenticated = HttpContext.User.Identity.IsAuthenticated;
            for (int i = dlvm.Repositories.Count() - 1; i >= 0; i--)
            {
                bool canRead = false;
                if (isAuthenticated)
                {
                    canRead = AuthService.CanRead(dlvm.Repositories[i].Name, HttpContext.User.Identity.Name);
                }
                if (!AuthService.IsRepositoryPublic(dlvm.Repositories[i].Name) && !canRead)
                {
                    dlvm.Repositories.RemoveAt(i);
                }
            }
            return View(dlvm);
        }

        [HttpPost]
        [FormAuthorizeEventually]
        public ActionResult Create(string project)
        {
            if (!AuthService.IsAdmin(HttpContext.User.Identity.Name) || !HttpContext.User.Identity.IsAuthenticated)
            {
                return new HttpUnauthorizedResult();
            }
            if (!string.IsNullOrEmpty(project))
            {
                repositories.CreateRepository(project);
            }
            return RedirectToAction("Index");
        }

        [HttpGet]
        [FormAuthorizeEventually]
        public ActionResult Delete(string project)
        {
            if (!AuthService.IsAdmin(HttpContext.User.Identity.Name) || !HttpContext.User.Identity.IsAuthenticated)
            {
                return new HttpUnauthorizedResult();
            }
            return View();
        }
    }
}