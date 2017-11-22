using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;

namespace SecurityManager
{
    public enum AuditEventTypes
    {
        UserAuthenticationSuccess = 0,
        UserAuthorizationSuccess = 1,
        UserAuthorizationFailed = 2,
        TicketSent = 3,
        TicketSentFailed = 4,
        AddUser = 5,
        AddUserFailed = 6,
        DeleteUser = 7,
        DeleteUserFailed = 8,
        EditUser = 9,
        EditUserFailed = 10,
        Deposit = 11,
        DepositFailed = 12,
        CreateAccount = 13,
        CreateAccountFailed = 14,
        LogIn = 15,
        LogInFailed = 16
    }

    public class AuditEvents
    {
        private static ResourceManager resourceManager = null;
        private static object resourceLock = new object();

        private static ResourceManager ResourceMgr
        {
            get
            {
                lock (resourceLock)
                {
                    if (resourceManager == null)
                    {
                        resourceManager = new ResourceManager(typeof(AuditEventsFile).FullName, Assembly.GetExecutingAssembly());
                    }
                    return resourceManager;
                }
            }
        }

        public static string UserAuthenticationSuccess
        {
            get
            {
                return ResourceMgr.GetString(AuditEventTypes.UserAuthenticationSuccess.ToString());
            }
        }

        public static string UserAuthorizationSuccess
        {
            get
            {
                return ResourceMgr.GetString(AuditEventTypes.UserAuthorizationSuccess.ToString());
            }
        }

        public static string UserAuthorizationFailed
        {
            get
            {
                return ResourceMgr.GetString(AuditEventTypes.UserAuthorizationFailed.ToString());
            }
        }

        public static string TicketSent
        {
            get
            {
                return ResourceMgr.GetString(AuditEventTypes.TicketSent.ToString());
            }
        }
        public static string TicketSentFailed
        {
            get
            {
                return ResourceMgr.GetString(AuditEventTypes.TicketSentFailed.ToString());
            }
        }

        public static string AddUser
        {
            get
            {
                return ResourceMgr.GetString(AuditEventTypes.AddUser.ToString());
            }
        }

        public static string AddUserFailed
        {
            get
            {
                return ResourceMgr.GetString(AuditEventTypes.AddUserFailed.ToString());
            }
        }
        public static string EditUser
        {
            get
            {
                return ResourceMgr.GetString(AuditEventTypes.EditUser.ToString());
            }
        }

        public static string EditUserFailed
        {
            get
            {
                return ResourceMgr.GetString(AuditEventTypes.EditUserFailed.ToString());
            }
        }

        public static string DeleteUser
        {
            get
            {
                return ResourceMgr.GetString(AuditEventTypes.DeleteUser.ToString());
            }
        }

        public static string DeleteUserFailed
        {
            get
            {
                return ResourceMgr.GetString(AuditEventTypes.DeleteUserFailed.ToString());
            }
        }

        public static string Deposit
        {
            get
            {
                return ResourceMgr.GetString(AuditEventTypes.Deposit.ToString());
            }
        }

        public static string DepositFailed
        {
            get
            {
                return ResourceMgr.GetString(AuditEventTypes.DepositFailed.ToString());
            }
        }

        public static string CreateAccount
        {
            get
            {
                return ResourceMgr.GetString(AuditEventTypes.CreateAccount.ToString());
            }
        }

        public static string CreateAccountFailed
        {
            get
            {
                return ResourceMgr.GetString(AuditEventTypes.CreateAccountFailed.ToString());
            }
        }
        public static string LogIn
        {
            get
            {
                return ResourceMgr.GetString(AuditEventTypes.LogIn.ToString());
            }
        }
        public static string LogInFailed
        {
            get
            {
                return ResourceMgr.GetString(AuditEventTypes.LogInFailed.ToString());
            }
        }
    }
}

