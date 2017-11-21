using Contracts;
using SecurityManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IntergrationPlatform
{
    public class BankService : IBankService
    {
        BankServiceProxy proxy;

        public BankService()
        {
            NetTcpBinding binding = new NetTcpBinding();
            string address = "net.tcp://localhost:9000/BankService";
            proxy = new BankServiceProxy(binding, address);
        }

        public bool BankLogin(string username, string password, int port)
        {
            bool allowed = false;

            CustomPrincipal principal = Thread.CurrentPrincipal as CustomPrincipal;
            if (principal.IsInRole("User") || principal.IsInRole("Reader") || principal.IsInRole("BankAdmin"))
            {
                Audit.AuthenticationSuccess(principal.Identity.Name.Split('\\')[1].ToString());
                proxy.BankLogin(username, password, port);
                allowed = true;
            }

            else
                Console.WriteLine("Deposit() failed for user {0}.", principal.Identity.Name);
            return allowed;
        }

        public bool CheckIfAlive()
        {
            return proxy.CheckIfAlive();
        }

        public bool Deposit(Account acc, string username)
        {
            bool allowed = false;

            CustomPrincipal principal = Thread.CurrentPrincipal as CustomPrincipal;
            if (principal.IsInRole("User") || principal.IsInRole("Reader"))
            {
                Audit.AuthorizationSuccess(principal.Identity.Name.Split('\\')[1].ToString(), "Deposite");
                proxy.Deposit(acc, username);
                Audit.Deposit(principal.Identity.Name.Split('\\')[1].ToString(), acc.Number.ToString());
                allowed = true;
            }
            else
                Audit.AuthorizationFailed(principal.Identity.Name.Split('\\')[1].ToString(), "Deposit","not authorized");
                Audit.DepositFailed(principal.Identity.Name.Split('\\')[1].ToString(), acc.Number.ToString(),"not authorized");
                Console.WriteLine("Deposit() failed for user {0}.", principal.Identity.Name);
                return allowed;
        }

        public bool CreateAccount(User user)
        {
            bool allowed = false;

            CustomPrincipal principal = Thread.CurrentPrincipal as CustomPrincipal;
            if (principal.IsInRole("BankAdmin"))
            {
                Audit.AuthorizationSuccess(principal.Identity.Name.Split('\\')[1].ToString(), "create account");

                proxy.CreateAccount(user);
                Audit.CreateAccount(principal.Identity.Name.Split('\\')[1].ToString());
                allowed = true;
            }
            else
                Audit.AuthorizationFailed(principal.Identity.Name.Split('\\')[1].ToString(), "create account", "not authorized");
                Audit.CreateAccountFailed(principal.Identity.Name.Split('\\')[1].ToString(), "not authorized");
                Console.WriteLine("Deposit() failed for user {0}.", principal.Identity.Name);
            

            return allowed;
        }
    }
}
