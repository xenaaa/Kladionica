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
                proxy.Deposit(acc, username);
                allowed = true;
            }
            else
                Console.WriteLine("Deposit() failed for user {0}.", principal.Identity.Name);
            return allowed;
        }

        public bool CreateAccount(User user)
        {
            bool allowed = false;

            CustomPrincipal principal = Thread.CurrentPrincipal as CustomPrincipal;
            if (principal.IsInRole("BankAdmin"))
            {
                proxy.CreateAccount(user);
                allowed = true;
            }
            else
                Console.WriteLine("Deposit() failed for user {0}.", principal.Identity.Name);
            return allowed;
        }
    }
}
