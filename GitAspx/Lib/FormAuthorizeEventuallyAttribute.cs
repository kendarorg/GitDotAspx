using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Security.Principal;
using System.Web.Security;

namespace GitAspx.Lib
{
    public class FormAuthorizeEventuallyAttribute:AuthorizeAttribute
    {
        public FormAuthorizeEventuallyAttribute()
        {
            IsEventual = false;
        }
        public bool IsEventual { get; set; }
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            if (httpContext == null)
            {
                throw new ArgumentNullException("httpContext");
            }

            IPrincipal user = httpContext.User;
            if (!user.Identity.IsAuthenticated)
            {
                if (httpContext.Request.Cookies[FormsAuthentication.FormsCookieName] != null)
                {
                    string cookie = httpContext.Request.Cookies[FormsAuthentication.FormsCookieName].Value;
                    FormsAuthenticationTicket ticket = FormsAuthentication.Decrypt(cookie);
                    if (AuthService.ValidateUser(ticket.Name))
                    {
                        httpContext.User = new GenericPrincipal(new GenericIdentity(ticket.Name), null);
                        user = httpContext.User;
                    }
                }
            }

            if (!AuthService.ValidateUser(user.Identity.Name))
            {
                if(!IsEventual) return false;
            }

            return true;
        }

        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            if (filterContext == null)
            {
                throw new ArgumentNullException("filterContext");
            }

            if (AuthorizeCore(filterContext.HttpContext))
            {
                // ** IMPORTANT **
                // Since we're performing authorization at the action level, the authorization code runs
                // after the output caching module. In the worst case this could allow an authorized user
                // to cause the page to be cached, then an unauthorized user would later be served the
                // cached page. We work around this by telling proxies not to cache the sensitive page,
                // then we hook our custom authorization code into the caching mechanism so that we have
                // the final say on whether a page should be served from the cache.

                HttpCachePolicyBase cachePolicy = filterContext.HttpContext.Response.Cache;
                cachePolicy.SetProxyMaxAge(new TimeSpan(0));
                cachePolicy.AddValidationCallback(CacheValidateHandler, null /* data */);
            }
            else
            {
                filterContext.Result = new HttpStatusCodeResult(401);
            }

        }

        private void CacheValidateHandler(HttpContext context, object data, ref HttpValidationStatus validationStatus)
        {
            validationStatus = OnCacheAuthorization(new HttpContextWrapper(context));
        }
    }
}