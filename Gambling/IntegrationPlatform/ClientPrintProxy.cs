using Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationPlatform
{
    public class ClientPrintProxy : ChannelFactory<IClientPrint>, IClientPrint, IDisposable
    {
        IClientPrint factory;

        public ClientPrintProxy() { }
        public ClientPrintProxy(NetTcpBinding binding, string address) : base(binding, address)
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

        
    }
}
