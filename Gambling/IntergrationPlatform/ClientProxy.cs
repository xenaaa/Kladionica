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
    public class ClientProxy : ChannelFactory<IClientHelper>, IClientHelper, IDisposable
    {
        IClientHelper factory;

        public ClientProxy() { }
        public ClientProxy(NetTcpBinding binding, string address) : base(binding, address)
        {
            factory = this.CreateChannel();

        }

        public bool CheckIfAlive(byte[] portBytes, byte[] addressBytes)
        {
            try
            {
                factory.CheckIfAlive(portBytes, addressBytes);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error {0}", e.Message);
                return false;
            }
        }

        public bool SendGameResults(byte[] results, byte[] port,byte[] address)
        {
            try
            {
                factory.SendGameResults(results, port, address);
                CustomPrincipal principal = Thread.CurrentPrincipal as CustomPrincipal;
                
                
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error {0}", e.Message);
                return false;
            }
        }

        //   public bool SendOffers(Dictionary<int, BetOffer> offers, int port)
        public bool SendOffers(byte[] offers, byte[] port, byte[] addressBytes)
        {
            try
            {
                factory.SendOffers(offers, port, addressBytes);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error {0}", e.Message);
                return false;
            }
        }

        public bool SendTicketResults(byte[] ticket, byte[] isPassed, byte[] port,byte[] address)
        {
            try
            {
                factory.SendTicketResults(ticket, isPassed, port, address);
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
