using CertificateManager;
using Contracts;
using SecurityManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IntegrationPlatform
{
    public class BetServiceProxy : ChannelFactory<IBetService>, IBetService, IDisposable
    {
        IBetService factory;

        public BetServiceProxy() { }
        public BetServiceProxy(NetTcpBinding binding, EndpointAddress address) : base(binding, address)
        {
            /// cltCertCN.SubjectName should be set to the client's username. .NET WindowsIdentity class provides information about Windows user running the given process
            string cltCertCN = "betserviceclient"; //mijenjala

            this.Credentials.ServiceCertificate.Authentication.CertificateValidationMode = System.ServiceModel.Security.X509CertificateValidationMode.ChainTrust;
            this.Credentials.ServiceCertificate.Authentication.RevocationMode = X509RevocationMode.NoCheck;

            /// Set appropriate client's certificate on the channel. Use CertManager class to obtain the certificate based on the "cltCertCN"
            this.Credentials.ClientCertificate.Certificate = CertManager.GetCertificateFromStorage(StoreName.My, StoreLocation.LocalMachine, cltCertCN);
            //  Console.WriteLine(this.Credentials.ClientCertificate.Certificate.ToString());
            factory = this.CreateChannel();
        }

        public bool AddUser(byte[] user)
        {
            try
            {
                return factory.AddUser(user);
            }
            catch (Exception e)
            {

                Console.WriteLine("Error {0}", e.Message);
                return false;
            }
        }


        public bool DeleteUser(byte[] username)
        {
            try
            {
                return factory.DeleteUser(username);

            }
            catch (Exception e)
            {

                Console.WriteLine("Error {0}", e.Message);
                return false;
            }
        }

        public bool EditUser(byte[] user)
        {
            try
            {
                return factory.EditUser(user);
            }
            catch (Exception e)
            {

                Console.WriteLine("Error {0}", e.Message);
                return false;
            }
        }

        public bool SendTicket(byte[] ticket, byte[] username)
        {

            try
            {
                return factory.SendTicket(ticket, username);
            }
            catch (Exception e)
            {

                Console.WriteLine("Error while trying to SendTicket(). {0}", e.Message);
                return false;
            }

        }

        public bool BetLogin(byte[] username, byte[] password, byte[] port)
        {
            try
            {
                return factory.BetLogin(username, password, port);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error {0}", e.Message);
                return false;
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



        public bool SendPort(byte[] username, byte[] port, byte[] address,byte[] printPort)
        {
            try
            {
                factory.SendPort(username, port, address, printPort);
                return true;
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
                factory.Deposit(acc, username);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error {0}", e.Message);
                return false;
            }
        }


        public bool IntrusionPrevention(byte[] user)
        {
            try
            {
                return factory.IntrusionPrevention(user);
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
    }
}