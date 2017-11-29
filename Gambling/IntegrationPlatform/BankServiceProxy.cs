using CertificateManager;
using Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationPlatform
{
    public class BankServiceProxy : ChannelFactory<IBankService>, IBankService, IDisposable
    {
        IBankService factory;

        public BankServiceProxy() { }

        public BankServiceProxy(NetTcpBinding binding, EndpointAddress address) : base(binding, address)
        {
            /// cltCertCN.SubjectName should be set to the client's username. .NET WindowsIdentity class provides information about Windows user running the given process
            string cltCertCN = "bankserviceclient";

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


        public bool BankLogin(byte[] username, byte[] password, byte[] address)
        {
            try
            {
                return factory.BankLogin(username, password, address);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error {0}", e.Message);
                return false;
            }
        }

        public bool CheckIfAlive(int port)
        {
            try
            {
                return factory.CheckIfAlive(port);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error {0}", e.Message);
                return false;
            }
        }

        public bool CreateAccount(byte[] user)
        {
            try
            {
                return factory.CreateAccount(user);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error {0}", e.Message);
                return false;
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
    }
}