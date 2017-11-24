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

        public bool CheckIfAlive(byte[] portBytes, byte[] addressBytes, byte[] isItPrintClientBytes)
        {

            try
            {
                return factory.CheckIfAlive(portBytes, addressBytes, isItPrintClientBytes);

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

        public bool GetServiceIP(byte[] AddressStringBytes)
        {
            throw new NotImplementedException();
        }

        public bool SendGameResults(byte[] results, byte[] port, byte[] address)
        {
            try
            {
                return factory.SendGameResults(results, port, address);

            }
            catch (Exception e)
            {
                Console.WriteLine("Error {0}", e.Message);
                return false;
            }
        }

        //   public bool SendOffers(Dictionary<int, BetOffer> offers, int port)
        public bool SendOffers(byte[] offers, byte[] port, byte[] addressBytes, byte[] isItPrintClientBytes)
        {
            try
            {
                return factory.SendOffers(offers, port, addressBytes, isItPrintClientBytes);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error {0}", e.Message);
                return false;
            }
        }

        public bool SendTicketResults(byte[] ticket, byte[] isPassed, byte[] port, byte[] address)
        {
            try
            {
                return factory.SendTicketResults(ticket, isPassed, port, address);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error {0}", e.Message);
                return false;
            }
        }
    }
}