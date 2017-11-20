using Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace IntergrationPlatform
{
    public class ClientHelper : IClientHelper
    {
        ClientProxy proxy;

        public ClientHelper()
        {

        }

        public bool CheckIfAlive(int port)
        {
            NetTcpBinding binding = new NetTcpBinding();
            string address = "net.tcp://localhost:" + port + "/ClientHelper";
            proxy = new ClientProxy(binding, address);
            return proxy.CheckIfAlive(port);
        }

        public bool SendGameResults(List<Game> results, int port)
        {
            NetTcpBinding binding = new NetTcpBinding();
            string address = "net.tcp://localhost:" + port + "/ClientHelper";
            proxy = new ClientProxy(binding, address);
            return proxy.SendGameResults(results, port);
        }

        public bool SendOffers(Dictionary<int, BetOffer> offers, int port)
        {
            NetTcpBinding binding = new NetTcpBinding();
            string address = "net.tcp://localhost:" + port + "/ClientHelper";
            proxy = new ClientProxy(binding, address);
            return proxy.SendOffers(offers, port);
        }

        public bool SendTicketResults(Ticket ticket, bool isPassed, int port)
        {
            NetTcpBinding binding = new NetTcpBinding();
            string address = "net.tcp://localhost:" + port + "/ClientHelper";
            proxy = new ClientProxy(binding, address);
            return proxy.SendTicketResults(ticket, isPassed, port);
        }
    }
}