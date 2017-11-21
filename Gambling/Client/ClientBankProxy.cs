using Contracts;
using SecurityManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Client
{

    public class ClientBankProxy : ChannelFactory<IBankService>, IBankService, IDisposable
    {
        IBankService factory;

        public ClientBankProxy(NetTcpBinding binding, string address) : base(binding, address)
        {
            factory = this.CreateChannel();
        }

        public bool CheckIfAlive()
        {
            try
            {
                factory.CheckIfAlive();
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error {0}", e.Message);
                return false;
            }
        }

        public bool BankLogin(string username, string password, int port)
        {
            try
            {
                factory.BankLogin(username, password, port);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error {0}", e.Message);
                return false;
            }
        }

        public bool Deposit(Account acc, string username)
        {
            CustomPrincipal principal = Thread.CurrentPrincipal as CustomPrincipal;
            try
            {
                factory.Deposit(acc, username);
                Audit.Deposit(principal.Identity.Name.Split('\\')[1].ToString(), acc.Number.ToString());
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error {0}", e.Message);
                return false;
            }
        }

        public bool CreateAccount(User user)
        {
            CustomPrincipal principal = Thread.CurrentPrincipal as CustomPrincipal;
            try
            {
                factory.CreateAccount(user);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error {0}", e.Message);
                return false;
            }
        }
    }
}
