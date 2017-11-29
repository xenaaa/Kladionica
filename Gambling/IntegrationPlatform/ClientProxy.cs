using Contracts;
using SecurityManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IntegrationPlatform
{
    public class ClientProxy : ChannelFactory<IClientHelper>, IClientHelper, IDisposable
    {
        IClientHelper factory;

        public ClientProxy() { }
        public ClientProxy(NetTcpBinding binding, string address) : base(binding, address)
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

        public bool CloseProxy()
        {
            try
            {
                return factory.CloseProxy();

            }
            catch (Exception e)
            {
                Console.WriteLine("Error {0}", e.Message);
                return false;
            }
        }

        public bool SendOffers(byte[] offers)
        {
            try
            {
                return factory.SendOffers(offers);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error {0}", e.Message);
                return false;
            }
        }

        public bool SendTicketResults(byte[] ticket, byte[] isPassed)
        {
            try
            {
                return factory.SendTicketResults(ticket, isPassed);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error {0}", e.Message);
                return false;
            }
        }
    }
}