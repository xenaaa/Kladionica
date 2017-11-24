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
            try
            {
                factory = this.CreateChannel();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public bool CheckIfAlive()
        {
            try
            {
                return factory.CheckIfAlive();
            }
            catch (Exception e)
            {
                Console.WriteLine("Error {0}", e.Message);
                return false;
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

        public bool Deposit(byte[] acc, byte[] username)
        {
            CustomPrincipal principal = Thread.CurrentPrincipal as CustomPrincipal;
            try
            {
                return factory.Deposit(acc, username);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error {0}", e.Message);
                return false;
            }
        }

        public bool CreateAccount(byte[] user)
        {
            CustomPrincipal principal = Thread.CurrentPrincipal as CustomPrincipal;
            try
            {
                return factory.CreateAccount(user);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error {0}", e.Message);
                return false;
            }
        }

        public bool IntrusionPrevention(byte[] user)
        {
            throw new NotImplementedException();
        }
    }
}