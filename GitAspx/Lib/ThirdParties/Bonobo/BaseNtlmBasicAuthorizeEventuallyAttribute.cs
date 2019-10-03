using System;
using System.Security.Principal;
using System.Text;
using System.Web;
using System.Web.Mvc;

using System.Threading;

namespace MultiAuthLib.ThirdParties.Bonobo
{
    public abstract class BaseNtlmBasicAuthorizeEventuallyAttribute : AuthorizeAttribute
    {
        protected class UserData
        {
            public string UserName { get; set; }
            public string Password { get; set; }
        }

        protected BaseNtlmBasicAuthorizeEventuallyAttribute()
        {
            IsEventual = false;
        }

        public bool IsEventual { get; set; }

        protected virtual bool ValidateUser(string name, string password)
        {
            return false;
        }

        protected virtual string GetUserPassword(string name)
        {
            return null;
        }

        /*protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            OnAuthorizationx(filterContext);
        }

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            return false;
        }*/

        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            if (filterContext == null)
            {
                throw new ArgumentNullException("filterContext");
            }
            string auth = filterContext.HttpContext.Request.Headers["Authorization"];
            if (!String.IsNullOrEmpty(auth))
            {
                var userData = new UserData();
                var authType = auth.ToLowerInvariant();
                if (authType.StartsWith("basic "))
                {
                    AuthWithBasic(auth, userData);
                }

                if (ValidateUser(userData.UserName, userData.Password))
                {
                    filterContext.HttpContext.User = new GenericPrincipal(new GenericIdentity(userData.UserName), null);
                }
                else
                {
                    if(!IsEventual) filterContext.Result = new HttpStatusCodeResult(401);
                }
            }
            else
            {
                if (AuthorizeCore(filterContext.HttpContext))
                {
                    HttpCachePolicyBase cachePolicy = filterContext.HttpContext.Response.Cache;
                    cachePolicy.SetProxyMaxAge(new TimeSpan(0));
                    cachePolicy.AddValidationCallback(CacheValidateHandler, null);
                }
                else
                {
                    //Basic auth is standard
                    ResponseUnauthorized(filterContext, "Basic realm=\"Secure Area\"");
                }
            }
        }

        private static void AuthWithBasic(string auth, UserData userData)
        {
            byte[] encodedDataAsBytes = Convert.FromBase64String(auth.Replace("Basic ", ""));
            string value = Encoding.ASCII.GetString(encodedDataAsBytes);
            if (!string.IsNullOrEmpty(value))
            {
                userData.UserName = value.Substring(0, value.IndexOf(':'));
                userData.Password = value.Substring(value.IndexOf(':') + 1);
            }
        }

       

        private static void ResponseUnauthorized(AuthorizationContext filterContext, string authHeaders, bool flush = false)
        {
            filterContext.HttpContext.Response.Clear();

            filterContext.HttpContext.Response.Headers["WWW-Authenticate"] = authHeaders;
            filterContext.HttpContext.Response.AddHeader("WWW-Authenticate", authHeaders);
            filterContext.HttpContext.Response.StatusDescription = "Unauthorized";
            filterContext.HttpContext.Response.Write("401, please authenticate");
            filterContext.HttpContext.Response.StatusCode = 401;
            if (!flush)
            {
                filterContext.Result = new EmptyResult();
                filterContext.HttpContext.Response.End();
            }
            else
            {
                filterContext.HttpContext.Response.Flush();
            }
            //Thread.Sleep(1000);
            //}
        }

        private void CacheValidateHandler(HttpContext context, object data, ref HttpValidationStatus validationStatus)
        {
            validationStatus = OnCacheAuthorization(new HttpContextWrapper(context));
        }
    }
}