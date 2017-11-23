using CertificateManager;
using Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.ServiceModel.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BankServer
{
    class Program
    {
        static int FreeTcpPort()
        {
            TcpListener l = new TcpListener(IPAddress.Loopback, 0);
            l.Start();
            int port = ((IPEndPoint)l.LocalEndpoint).Port;
            l.Stop();
            return port;
        }
        static void Main(string[] args)
        {
            Thread.Sleep(4000);


            string srvCertCN = "bankservice";

            NetTcpBinding binding = new NetTcpBinding();
            binding.Security.Transport.ClientCredentialType = TcpClientCredentialType.Certificate;
            //  string address = "net.tcp://localhost:" + Helper.bankServicePort + "/BankService";
            string address = "net.tcp://localhost:" + FreeTcpPort() + "/BankService";


            ServiceHost host = new ServiceHost(typeof(BankService));
            host.AddServiceEndpoint(typeof(IBankService), binding, address);

            //SERTIFIFIKACIJA
            host.Credentials.ClientCertificate.Authentication.CertificateValidationMode = X509CertificateValidationMode.ChainTrust;
            host.Credentials.ClientCertificate.Authentication.RevocationMode = X509RevocationMode.NoCheck;

            ///Set appropriate service's certificate on the host. Use CertManager class to obtain the certificate based on the "srvCertCN"
            host.Credentials.ServiceCertificate.Certificate = CertManager.GetCertificateFromStorage(StoreName.My, StoreLocation.LocalMachine, srvCertCN);

            host.Open();

            string srvCertCN2 = "bankserviceintegration";
            binding = new NetTcpBinding();
            binding.Security.Transport.ClientCredentialType = TcpClientCredentialType.Certificate;

            X509Certificate2 srvCert = CertManager.GetCertificateFromStorage(StoreName.My, StoreLocation.LocalMachine, srvCertCN2);
            EndpointAddress address2 = new EndpointAddress(new Uri("net.tcp://" + Helper.integrationHostAddress + ":" + Helper.integrationHostPort + "/BetIntegrationPlatform2"),
                                      new X509CertificateEndpointIdentity(srvCert));

            BankServerProxy proxy;
            proxy = new BankServerProxy(binding, address2);

            string IP=string.Empty;
            var hostIP = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in hostIP.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    IP= ip.ToString();
                }
            }

            address=address.Replace("localhost", IP);


            proxy.GetServiceIP(Helper.Encrypt(address));


            Console.WriteLine("Bank service is started.");
            Console.WriteLine("Press <enter> to stop service...");

            Console.ReadLine();
            host.Close();
        }
    }
}
