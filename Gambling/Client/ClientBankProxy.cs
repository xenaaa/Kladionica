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

        public bool CreateAccount(User user)
        {
            throw new NotImplementedException();
        }

        public bool Deposit(Account acc, User user)
        {
            throw new NotImplementedException();
        }
    }
}
