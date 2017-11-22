using CertificateManager;
using Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.ServiceModel.Security;
using System.Text;
using System.Threading.Tasks;

namespace BankServer
{
    class Program
    {
        static void Main(string[] args)
        {
            string srvCertCN = "bankservice";

            NetTcpBinding binding = new NetTcpBinding();
            binding.Security.Transport.ClientCredentialType = TcpClientCredentialType.Certificate;
            string address = "net.tcp://localhost:"+ Helper.bankServicePort + "/BankService";

            ServiceHost host = new ServiceHost(typeof(BankService));
            host.AddServiceEndpoint(typeof(IBankService), binding, address);

            //SERTIFIFIKACIJA
            host.Credentials.ClientCertificate.Authentication.CertificateValidationMode = X509CertificateValidationMode.ChainTrust;
            host.Credentials.ClientCertificate.Authentication.RevocationMode = X509RevocationMode.NoCheck;

            ///Set appropriate service's certificate on the host. Use CertManager class to obtain the certificate based on the "srvCertCN"
            host.Credentials.ServiceCertificate.Certificate = CertManager.GetCertificateFromStorage(StoreName.My, StoreLocation.LocalMachine, srvCertCN);

            host.Open();

            Console.WriteLine("Bank service is started.");
            Console.WriteLine("Press <enter> to stop service...");

            Console.ReadLine();
            host.Close();
        }
    }
}
