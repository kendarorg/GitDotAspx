using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MultiAuthLib.ThirdParties.Bonobo;

namespace GitAspx.Lib
{
    public class BasicAuthorizeEventuallyAttribute : BaseNtlmBasicAuthorizeEventuallyAttribute
    {
        protected override bool ValidateUser(string name, string password)
        {
            return AuthService.ValidateUser(name, password);
        }

        protected override string GetUserPassword(string name)
        {
            return AuthService.GetUserPassword(name);
        }
    }
}