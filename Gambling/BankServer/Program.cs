using CertificateManager;
using Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
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
        public static SHA512 shaHash;
        static void Main(string[] args)
        {
            shaHash = SHA512.Create();
            Persistance.EmptyBankFiles();
            string srvCertCN = "bankservice";

            NetTcpBinding binding = new NetTcpBinding();
            binding.Security.Transport.ClientCredentialType = TcpClientCredentialType.Certificate;
            int port = FreeTcpPort();

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

            X509Certificate2 srvCert = CertManager.GetCertificateFromStorage(StoreName.TrustedPeople, StoreLocation.LocalMachine, srvCertCN2);
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

            BankService bs = new BankService();
            if (!bs.CreateFirstAccounts(new User("adminBank", GetSha512Hash(shaHash, "admin"), "BankAdmin")))
                Console.WriteLine("User already exists");
            if (!bs.CreateFirstAccounts(new User("marina", GetSha512Hash(shaHash, "marina"), "User")))
                Console.WriteLine("User already exists");
            if (!bs.CreateFirstAccounts(new User("bojan", GetSha512Hash(shaHash, "bojan"), "User")))
                Console.WriteLine("User already exists");
            if (!bs.CreateFirstAccounts(new User("david", GetSha512Hash(shaHash, "david"), "User")))
                Console.WriteLine("User already exists");
            if (!bs.CreateFirstAccounts(new User("nicpa", GetSha512Hash(shaHash, "nicpa"), "User")))
                Console.WriteLine("User already exists");
            if (!bs.CreateFirstAccounts(new User("djole", GetSha512Hash(shaHash, "djole"), "Reader")))
                Console.WriteLine("User already exists");


            Console.ReadLine();
            host.Close();
        }

        static string GetSha512Hash(SHA512 shaHash, string input)
        {
            // Convert the input string to a byte array and compute the hash.
            byte[] data = shaHash.ComputeHash(Encoding.UTF8.GetBytes(input));

            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string.
            return sBuilder.ToString();
        }
    }
}
