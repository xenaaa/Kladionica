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

namespace IntergrationPlatform
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

        public bool AddUser(User user)
        {
            
            try
            {
                factory.AddUser(user);
               
                return true;
            }
            catch (Exception e)
            {
              
                Console.WriteLine("Error {0}", e.Message);
                return false;
            }
        }


        public bool DeleteUser(string username)
        {
           
            try
            {

                factory.DeleteUser(username);
                
                return true;
            }
            catch (Exception e)
            {
                
                Console.WriteLine("Error {0}", e.Message);
                return false;
            }
        }

        public bool EditUser(User user)
        {
            
            try
            {
                factory.EditUser(user);
               
                return true;
            }
            catch (Exception e)
            {
               
                Console.WriteLine("Error {0}", e.Message);
                return false;
            }
        }

        public bool SendTicket(Ticket ticket, string username)
        {
            
            bool sent = false;
            try
            {
                sent = factory.SendTicket(ticket, username);
                Console.WriteLine("SendTicket() >> {0}", sent);
               
            }
            catch (Exception e)
            {
               
                Console.WriteLine("Error while trying to SendTicket(). {0}", e.Message);
            }

            return sent;
        }

        public bool BetLogin(string username, string password, int port)
        {
            try
            {
                factory.BetLogin(username, password, port);
                return true;
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
                factory.CheckIfAlive();
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error {0}", e.Message);
                return false;
            }
        }



        public bool SendPort(string username, int port)
        {
            try
            {
                factory.SendPort(username, port);
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