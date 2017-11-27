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
            try
            {
                factory = this.CreateChannel();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public bool AddUser(byte[] user, byte[] port)
        {
            try
            {
                return factory.AddUser(user,port);
            }
            catch (Exception e)
            {

                Console.WriteLine("Error {0}", e.Message);
                return false;
            }
        }


        public bool DeleteUser(byte[] username, byte[] port)
        {
            try
            {
                return factory.DeleteUser(username,port);

            }
            catch (Exception e)
            {

                Console.WriteLine("Error {0}", e.Message);
                return false;
            }
        }

        public bool EditUser(byte[] user, byte[] port)
        {
            try
            {
                return factory.EditUser(user,port);
            }
            catch (Exception e)
            {

                Console.WriteLine("Error {0}", e.Message);
                return false;
            }
        }

        public bool SendTicket(byte[] ticket, byte[] username, byte[] port)
        {

            try
            {
                return factory.SendTicket(ticket, username, port);
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



        public bool SendPort(byte[] username, byte[] port, byte[] address, byte[] printPort)
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

        public bool Deposit(byte[] acc, byte[] username, byte[] port)
        {
            try
            {
                factory.Deposit(acc, username,port);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error {0}", e.Message);
                return false;
            }
        }


        //public bool IntrusionPrevention(byte[] user)
        //{
        //    try
        //    {
        //        return factory.IntrusionPrevention(user);
        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine("Error {0}", e.Message);
        //        return false;
        //    }
        //}


        public bool GetServiceIP(byte[] AddressStringBytes)
        {
            throw new NotImplementedException();
        }

        public List<Dictionary<string, int>> Report()
        {
            throw new NotImplementedException();
        }

    }
}