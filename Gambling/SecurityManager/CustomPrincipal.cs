using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace SecurityManager
{
    public class CustomPrincipal : IPrincipal, IDisposable
    {
        private WindowsIdentity identity = null;
        private List<string> groups = new List<string>();

        public CustomPrincipal(WindowsIdentity winIdentity)
        {
            this.identity = winIdentity;

            /// define list of roles based on Windows groups (roles) 			 
            foreach (IdentityReference group in this.identity.Groups)
            {
                SecurityIdentifier sid = (SecurityIdentifier)group.Translate(typeof(SecurityIdentifier));
                var name = sid.Translate(typeof(NTAccount));
                string groupName = Formatter.GetName(name.ToString());    /// return name of the Windows group				

                if (!groups.Contains(groupName) && (groupName == "User" || groupName == "BankAdmin" || groupName == "BetAdmin" || groupName == "Reader"))
                    groups.Add(groupName);
            }
        }



        public IIdentity Identity
        {
            get { return this.identity; }
        }

        public bool IsInRole(string group)
        {
            if (groups.Contains(group))
                return true;
            return false;
        }


        public void Dispose()
        {
            if (identity != null)
            {
                identity.Dispose();
                identity = null;
            }
        }
    }
}
