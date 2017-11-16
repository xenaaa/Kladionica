using IntegrationPlatform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
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

        public bool Login(string username, string password)
        {
            try
            {
                factory.Login(username,password);
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
            try
            {
                factory.Deposit(acc,username);
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
