using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using System.Web.Security;
using System.IO;
using System.Web.Mvc;
using GitAspx.Config;

namespace GitAspx.Lib
{
    public static class AuthService
    {

        #region Methods to log in a user.
        /// <summary>
        /// Create the auth cookie in the same way it is created my ASP.NET Membership system, hopefully lasting for more than 20 minutes.
        /// 
        /// For more information check out http://stackoverflow.com/questions/2122831/is-it-possible-to-use-aspxauth-for-my-own-logging-system
        /// </summary>
        /// <param name="userId">Id of the user that is logged in</param>
        /// <returns>Cookie created to mark the user as authenticated.</returns>
        public static HttpCookie CreateAuthCookie(string userId)
        {
            DateTime issued = DateTime.Now;
            // formsAuth does not expose timeout!? have to hack around the spoiled parts and keep moving..
            HttpCookie fooCookie = FormsAuthentication.GetAuthCookie("foo", true);
            int formsTimeout = Convert.ToInt32((fooCookie.Expires - DateTime.Now).TotalMinutes);

            DateTime expiration = DateTime.Now.AddMinutes(formsTimeout);

            var ticket = new FormsAuthenticationTicket(0, userId, issued, expiration, true, "", FormsAuthentication.FormsCookiePath);
            return CreateAuthCookie(ticket, expiration, true);
        }

        /// <summary>
        /// Create an auth cookie with the ticket data.
        /// </summary>
        /// <param name="ticket">Ticket containing the data to mark a user as authenticated.</param>
        /// <param name="expiration">Expriation date for the cookie.</param>
        /// <param name="persistent">Whether it's persistent or not.</param>
        /// <returns>Cookie created to mark the user as authenticated.</returns>
        public static HttpCookie CreateAuthCookie(FormsAuthenticationTicket ticket, DateTime expiration, bool persistent)
        {
            string encryptedAuthData = FormsAuthentication.Encrypt(ticket);
            var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedAuthData)
            {
                Domain = FormsAuthentication.CookieDomain,
                Path = FormsAuthentication.FormsCookiePath
            };
            if (persistent)
            {
                cookie.Expires = expiration;
            }

            return cookie;
        }

        /// <summary>
        /// Expire the authentication cookie effectively loging out a user.
        /// </summary>
        public static void ExpireAuthCookie(HttpContextBase ctx)
        {
            var cookie = new HttpCookie(FormsAuthentication.FormsCookieName);
            cookie.Expires = DateTime.Now.AddDays(-1);
            ctx.Response.Cookies.Add(cookie);
        }
        #endregion

        public static bool ValidateUser(string login, string pwd)
        {
            var section = (GitAspxConfig)ConfigurationManager.GetSection("GitAspxConfig");
            var users = section.Users.GetAll();
            login = login.ToLowerInvariant();
            for (int i = 0; i < users.Count; i++)
            {
                var user = users[i];
                if (user.UserId == login && user.Password == pwd) return true;
            }
            return false;
        }

        public static bool CanRead(string repoId, string uid)
        {
            var section = (GitAspxConfig)ConfigurationManager.GetSection("GitAspxConfig");
            var repos = section.Repositories.GetAll();
            repoId = repoId.ToLowerInvariant();
            uid = uid.ToLowerInvariant();
            if (IsAdmin(uid)) return true;
            for (int i = 0; i < repos.Count; i++)
            {
                var repo = repos[i].RepoId.ToLowerInvariant();
                if (repo == repoId)
                {
                    if (repos[i].Read == "*") return true;
                    var users = repos[i].Read.Split(';');
                    foreach (var user in users)
                    {
                        if (uid == user.ToLowerInvariant()) return true;
                    }
                    return false;
                }
            }
            return false;
        }


        public static bool CanWrite(string repoId, string uid)
        {
            var section = (GitAspxConfig)ConfigurationManager.GetSection("GitAspxConfig");
            var repos = section.Repositories.GetAll();
            repoId = repoId.ToLowerInvariant();
            uid = uid.ToLowerInvariant();
            if (IsAdmin(uid)) return true;
            for (int i = 0; i < repos.Count; i++)
            {
                var repo = repos[i].RepoId.ToLowerInvariant();
                if (repo == repoId)
                {
                    var users = repos[i].Write.Split(';');
                    foreach (var user in users)
                    {
                        if (uid == user.ToLowerInvariant()) return true;
                    }
                    return false;
                }
            }
            return false;
        }

        public static bool IsRepositoryPublic(string repoId)
        {
            var section = (GitAspxConfig)ConfigurationManager.GetSection("GitAspxConfig");
            var repos = section.Repositories.GetAll();
            repoId = repoId.ToLowerInvariant();
            for (int i = 0; i < repos.Count; i++)
            {
                var repo = repos[i].RepoId.ToLowerInvariant();
                if (repo == repoId) return repos[i].Read=="*";
            }
            return false;
        }

        public static bool ValidateUser(string login)
        {
            var section = (GitAspxConfig)ConfigurationManager.GetSection("GitAspxConfig");
            var users = section.Users.GetAll();
            login = login.ToLowerInvariant();
            for (int i = 0; i < users.Count; i++)
            {
                var user = users[i];
                if (user.UserId == login) return true;
            }
            return false;
        }
        public static ActionResult RequireBasicAuth(HttpContextBase ctx)
        {
            string auth = ctx.Request.Headers["Authorization"];
            ctx.Response.Clear();
            ctx.Response.AddHeader("WWW-Authenticate", "Basic realm=\"Secure Area\"");
            ctx.Response.StatusCode = 401;
            ctx.Response.End();
            return new EmptyResult();
        }

        internal static string GetUserPassword(string name)
        {
            return ConfigurationManager.AppSettings["Password"];
        }

        internal static bool IsAdmin(string name)
        {
            name = name.ToLowerInvariant();
            var section = (GitAspxConfig)ConfigurationManager.GetSection("GitAspxConfig");
            return name == section.Admin.ToLowerInvariant();
        }
    }
}