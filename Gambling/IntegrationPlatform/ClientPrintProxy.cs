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

       

        public bool SendGameResults(byte[] results)
        {
            try
            {
                return factory.SendGameResults(results);

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

        
    }
}
