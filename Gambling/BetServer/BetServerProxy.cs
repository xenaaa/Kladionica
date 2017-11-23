using CertificateManager;
using Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace BetServer
{
    public class BetServerProxy : ChannelFactory<IClientHelper>, IClientHelper, IDisposable
    {
        IClientHelper factory;

        public BetServerProxy(NetTcpBinding binding, EndpointAddress address) : base(binding, address)
        {

            string cltCertCN = "betserviceclientintegration"; //mijenjala

            this.Credentials.ServiceCertificate.Authentication.CertificateValidationMode = System.ServiceModel.Security.X509CertificateValidationMode.ChainTrust;
            this.Credentials.ServiceCertificate.Authentication.RevocationMode = X509RevocationMode.NoCheck;

            /// Set appropriate client's certificate on the channel. Use CertManager class to obtain the certificate based on the "cltCertCN"
            this.Credentials.ClientCertificate.Certificate = CertManager.GetCertificateFromStorage(StoreName.My, StoreLocation.LocalMachine, cltCertCN);
            //  Console.WriteLine(this.Credentials.ClientCertificate.Certificate.ToString());
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

        public bool GetServiceIP(byte[] AddressStringBytes)
        {
            try
            {
                factory.GetServiceIP(AddressStringBytes);
                return true;
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
                factory.SendGameResults(results, port,address);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error {0}", e.Message);
                return false;
            }

        }

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