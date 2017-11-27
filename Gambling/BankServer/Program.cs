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
            Persistance.EmptyBankFiles();
            string srvCertCN = "bankservice";

            NetTcpBinding binding = new NetTcpBinding();
            binding.Security.Transport.ClientCredentialType = TcpClientCredentialType.Certificate;
            //  string address = "net.tcp://localhost:" + Helper.bankServicePort + "/BankService";
            int port = FreeTcpPort();
            Console.WriteLine(port);
            string address = "net.tcp://localhost:" + port + "/BankService";


            ServiceHost host = new ServiceHost(typeof(BankService));
            host.AddServiceEndpoint(typeof(IBankService), binding, address);

            //SERTIFIFIKACIJA
            host.Credentials.ClientCertificate.Authentication.CertificateValidationMode = X509CertificateValidationMode.ChainTrust;
            host.Credentials.ClientCertificate.Authentication.RevocationMode = X509RevocationMode.NoCheck;

            ///Set appropriate service's certificate on the host. Use CertManager class to obtain the certificate based on the "srvCertCN"
            host.Credentials.ServiceCertificate.Certificate = CertManager.GetCertificateFromStorage(StoreName.My, StoreLocation.LocalMachine, srvCertCN);
            try
            {
                host.Open();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }


            string srvCertCN2 = "bankserviceintegration";
            binding = new NetTcpBinding();
            binding.Security.Transport.ClientCredentialType = TcpClientCredentialType.Certificate;

            X509Certificate2 srvCert = CertManager.GetCertificateFromStorage(StoreName.My, StoreLocation.LocalMachine, srvCertCN2);
            EndpointAddress address2 = new EndpointAddress(new Uri("net.tcp://" + Helper.integrationHostAddress + ":" + Helper.integrationHostPort + "/BetIntegrationPlatform2"),
                                      new X509CertificateEndpointIdentity(srvCert));

            BankServerProxy proxy;


            string IP = string.Empty;
            var hostIP = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in hostIP.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    IP = ip.ToString();
                }
            }

            address = address.Replace("localhost", IP);

            while (true)
            {
                proxy = new BankServerProxy(binding, address2);
                if (proxy.GetServiceIP(Helper.Encrypt(address)))
                {
                    proxy.Close();
                    break;
                }
                else
                {
                    Console.WriteLine("Server not responding!");
                    proxy.Abort();
                    Thread.Sleep(1000);
                    continue;
                }
            }

            Console.WriteLine("Bank service is started.");
            Console.WriteLine("Press <enter> to stop service...");

            Console.ReadLine();
            host.Close();
        }
    }
}
