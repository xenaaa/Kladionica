using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace SecurityManager
{
    public class CustomAuthorizationManager : ServiceAuthorizationManager
    {
        protected override bool CheckAccessCore(OperationContext operationContext)
        {
            return true;
            //bool authorized = false;

            //IPrincipal principal = operationContext.ServiceSecurityContext.AuthorizationContext.Properties["Principal"] as IPrincipal;

            //if (principal != null)
            //{
            //    authorized = (principal as CustomPrincipal).IsInRole(Permissions.View.ToString());

            //    if (authorized == false)
            //    {
            //        /// audit authorization failed event					
            //    }
            //    else
            //    {
            //        /// audit successfull authorization event
            //    }
            //}

            //return authorized;
        }
    }
}
