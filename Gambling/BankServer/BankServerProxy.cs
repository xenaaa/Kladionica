using CertificateManager;
using Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace BankServer
{
    public class BankServerProxy : ChannelFactory<IBetServiceOnBank>, IBetServiceOnBank, IDisposable
    {
        IBetServiceOnBank factory;

        public BankServerProxy(NetTcpBinding binding, EndpointAddress address) : base(binding, address)
        {

            string cltCertCN = "bankserviceclientintegration";

            this.Credentials.ServiceCertificate.Authentication.CertificateValidationMode = System.ServiceModel.Security.X509CertificateValidationMode.ChainTrust;
            this.Credentials.ServiceCertificate.Authentication.RevocationMode = X509RevocationMode.NoCheck;

            /// Set appropriate client's certificate on the channel. Use CertManager class to obtain the certificate based on the "cltCertCN"
            this.Credentials.ClientCertificate.Certificate = CertManager.GetCertificateFromStorage(StoreName.My, StoreLocation.LocalMachine, cltCertCN);

            try
            {
                factory = this.CreateChannel();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public bool Deposit(byte[] acc, byte[] username)
        {
            try
            {
                return factory.Deposit(acc, username);
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
                return factory.GetServiceIP(AddressStringBytes);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error {0}", e.Message);
                return false;
            }
        }
    }
}