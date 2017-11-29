using SecurityManager;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace SecurityManager
{
    public class Audit : IDisposable
    {

        private static EventLog customLog = null;
        const string SourceName = "SecurityManager.Audit";
        const string LogName = "MySecTest";

        static Audit()
        {

            try
            {
                /// create customLog handle
                /// 
                if (!EventLog.SourceExists(SourceName))
                {
                    EventLog.CreateEventSource(SourceName, LogName);
                }
                customLog = new EventLog(LogName, Environment.MachineName, SourceName);
            }
            catch (Exception e)
            {
                customLog = null;
                Console.WriteLine("Error while trying to create log handle. Error = {0}", e.Message);
            }
        }


        public static void AuthenticationSuccess(string userName)
        {

            if (customLog != null)
            {
                string UserAuthenticationSuccessMessage = AuditEvents.UserAuthenticationSuccess;
                UserAuthenticationSuccessMessage = string.Format(UserAuthenticationSuccessMessage, userName);
                customLog.WriteEntry(UserAuthenticationSuccessMessage);
                // string message -> create message based on UserAuthenticationSuccess and params
                // write message in customLog, EventLogEntryType is Information or SuccessAudit 
            }
            else
            {
                throw new ArgumentException(string.Format("Error while trying to write event (eventid = {0}) to event log.", (int)AuditEventTypes.UserAuthenticationSuccess));
            }
        }
        public static void AuthorizationSuccess(string userName, string serviceName)
        {

            string UserAuthorizationSuccess = AuditEvents.UserAuthorizationSuccess;
            UserAuthorizationSuccess = string.Format(UserAuthorizationSuccess, userName, serviceName);
            customLog.WriteEntry(UserAuthorizationSuccess);

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="serviceName"> should be read from the OperationContext as follows: OperationContext.Current.IncomingMessageHeaders.Action</param>
        /// <param name="reason">permission name</param>
        public static void AuthorizationFailed(string userName, string serviceName, string reason)
        {
            string UserAuthorizationFailed = AuditEvents.UserAuthorizationFailed;
            UserAuthorizationFailed = string.Format(UserAuthorizationFailed, userName, serviceName, reason);
            customLog.WriteEntry(UserAuthorizationFailed);
        }

        public static void TicketSent(string userName)
        {
            string TicketSent = AuditEvents.TicketSent;
            TicketSent = string.Format(TicketSent, userName);
            customLog.WriteEntry(TicketSent);
        }

        public static void TicketSentFailed(string userName, string reason)
        {
            string TicketSentFailed = AuditEvents.TicketSentFailed;
            TicketSentFailed = string.Format(TicketSentFailed, userName, reason);
            customLog.WriteEntry(TicketSentFailed);
        }

        public static void AddUser(string userName, string addedUser)
        {
            string AddUser = AuditEvents.AddUser;
            AddUser = string.Format(AddUser, userName, addedUser);
            customLog.WriteEntry(AddUser);
        }

        public static void AddUserFailed(string userName, string addedUser, string reason)
        {
            string AddUserFailed = AuditEvents.AddUserFailed;
            AddUserFailed = string.Format(AddUserFailed, userName, addedUser, reason);
            customLog.WriteEntry(AddUserFailed);
        }

        public static void EditUser(string userName, string editedUser)
        {
            string EditUser = AuditEvents.EditUser;
            EditUser = string.Format(EditUser, userName, editedUser);
            customLog.WriteEntry(EditUser);
        }

        public static void EditUserFailed(string userName, string editedUser, string reason)
        {
            string EditUserFailed = AuditEvents.EditUserFailed;
            EditUserFailed = string.Format(EditUserFailed, userName, editedUser, reason);
            customLog.WriteEntry(EditUserFailed);
        }

        public static void DeleteUser(string userName, string deletedUser)
        {
            string DeleteUser = AuditEvents.DeleteUser;
            DeleteUser = string.Format(DeleteUser, userName, deletedUser);
            customLog.WriteEntry(DeleteUser);
        }

        public static void DeleteUserFailed(string userName, string deletedUser, string reason)
        {
            string DeleteUserFailed = AuditEvents.DeleteUser;
            DeleteUserFailed = string.Format(DeleteUserFailed, userName, deletedUser, reason);
            customLog.WriteEntry(DeleteUserFailed);
        }

        public static void Deposit(string userName, string acc)
        {
            string Deposit = AuditEvents.Deposit;
            Deposit = string.Format(Deposit, userName, acc);
            customLog.WriteEntry(Deposit);
        }

        public static void DepositFailed(string userName, string acc, string reason)
        {
            string DepositFailed = AuditEvents.DepositFailed; //unusual suspect
            DepositFailed = string.Format(DepositFailed, userName, acc, reason);
            customLog.WriteEntry(DepositFailed);
        }

        public static void CreateAccount(string userName)
        {
            string CreateAccount = AuditEvents.CreateAccount; //unusual suspect
            CreateAccount = string.Format(CreateAccount, userName);
            customLog.WriteEntry(CreateAccount);
        }

        public static void CreateAccountFailed(string userName, string reason)
        {
            string CreateAccountFailed = AuditEvents.CreateAccount; //unusual suspect
            CreateAccountFailed = string.Format(CreateAccountFailed, userName, reason);
            customLog.WriteEntry(CreateAccountFailed);
        }
        public static void LogIn(string userName)
        {
            string logIn = AuditEvents.LogIn; //unusual suspect
            logIn = string.Format(logIn, userName);
            customLog.WriteEntry(logIn);
        }
        public static void LogInFailed(string userName, string reason)
        {
            string logInFailed = AuditEvents.LogInFailed; //unusual suspect
            logInFailed = string.Format(logInFailed, userName, reason);
            customLog.WriteEntry(logInFailed);
        }

        public void Dispose()
        {
            if (customLog != null)
            {
                customLog.Dispose();
                customLog = null;
            }
        }
    }
}
