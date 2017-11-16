using BetServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    public class ClientProxy : ChannelFactory<IBetService>, IBetService, IDisposable
    {
        IBetService factory;

        public ClientProxy(NetTcpBinding binding, string address) : base(binding, address)
        {
            factory = this.CreateChannel();
        }

   
    }
}
