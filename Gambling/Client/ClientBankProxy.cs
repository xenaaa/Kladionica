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

    public class ClientBankProxy : ChannelFactory<IBankServiceIntegration>, IBankServiceIntegration, IDisposable
    {
        IBankServiceIntegration factory;

        public ClientBankProxy(NetTcpBinding binding, string address) : base(binding, address)
        {
            try
            {
                factory = this.CreateChannel();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public bool BankLogin(byte[] username, byte[] password, byte[] port, byte[] address)
        {
            try
            {
                return factory.BankLogin(username, password, port, address);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error {0}", e.Message);
                return false;
            }
        }

        public bool Deposit(byte[] acc, byte[] username, byte[] port)
        {
            CustomPrincipal principal = Thread.CurrentPrincipal as CustomPrincipal;
            try
            {
                return factory.Deposit(acc, username,port);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error {0}", e.Message);
                return false;
            }
        }

        public bool CreateAccount(byte[] user, byte[] port)
        {
            CustomPrincipal principal = Thread.CurrentPrincipal as CustomPrincipal;
            try
            {
                return factory.CreateAccount(user,port);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error {0}", e.Message);
                return false;
            }
        }

        public List<Dictionary<string, int>> Report()
        {
            try
            {
                return factory.Report();
            }
            catch (Exception e)
            {
                Console.WriteLine("Error {0}", e.Message);
                return null;
            }
        }

        public bool CheckIfAlive(int port)
        {
            try
            {
                return factory.CheckIfAlive(port);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error {0}", e.Message);
                return false;
            }
        }
    }
}