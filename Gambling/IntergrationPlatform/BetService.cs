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
    public class BetService : IBetService
    {
        BetServiceProxy proxy;

        public BetService()
        {
            string srvCertCN = "betservice";

            NetTcpBinding binding = new NetTcpBinding();
            binding.Security.Transport.ClientCredentialType = TcpClientCredentialType.Certificate;
            //   string address = "net.tcp://localhost:9998/BetService";

            X509Certificate2 srvCert = CertManager.GetCertificateFromStorage(StoreName.My, StoreLocation.LocalMachine, srvCertCN);
            EndpointAddress address = new EndpointAddress(new Uri("net.tcp://localhost:9998/BetService"),
                                      new X509CertificateEndpointIdentity(srvCert));

            proxy = new BetServiceProxy(binding, address);
        }

        public bool AddUser(User user)
        {
            bool allowed = false;

            CustomPrincipal principal = Thread.CurrentPrincipal as CustomPrincipal;
            if (principal.IsInRole("BetAdmin"))
            {
               // Audit.AuthorizationSuccess(principal.Identity.Name.Split('\\')[1].ToString(), "AddUser");
                proxy.AddUser(user);
              //  Audit.AddUser(principal.Identity.Name.Split('\\')[1].ToString(), user.Username.ToString());
                allowed = true;
            }
            else
            {
              //  Audit.AuthorizationFailed(principal.Identity.Name.Split('\\')[1].ToString(), "AddUser","not authorized");
              //  Audit.AddUserFailed(principal.Identity.Name.Split('\\')[1].ToString(), user.Username.ToString(),"not authorized");
                Console.WriteLine("AddUser() failed for user {0}.", principal.Identity.Name);
            }
            return allowed;
        }

        public bool BetLogin(string username, string password, int port)
        {
            bool allowed = false;

            CustomPrincipal principal = Thread.CurrentPrincipal as CustomPrincipal;
            if (principal.IsInRole("User") || principal.IsInRole("Reader") || principal.IsInRole("BetAdmin"))
            {
              //  Audit.AuthenticationSuccess(principal.Identity.Name.Split('\\')[1].ToString());
                proxy.BetLogin(username, password, port);
                allowed = true;
            }

           // else
              //  Audit.AuthorizationFailed(principal.Identity.Name.Split('\\')[1].ToString(), "BetLogin", "not authorized");
            return allowed;
        }

        public bool CheckIfAlive()
        {
            return proxy.CheckIfAlive();
        }

        public bool DeleteUser(string username)
        {
            bool allowed = false;

            CustomPrincipal principal = Thread.CurrentPrincipal as CustomPrincipal;
            if (principal.IsInRole("BetAdmin"))
            {
              //  Audit.AuthorizationSuccess(principal.Identity.Name.Split('\\')[1].ToString(), "DeleteUser");
                proxy.DeleteUser(username);
               // Audit.DeleteUser(principal.Identity.Name.Split('\\')[1].ToString(), username);
                allowed = true;
            }
            else
               // Audit.AuthorizationFailed(principal.Identity.Name.Split('\\')[1].ToString(), "DeleteUser","not authorized");
               // Audit.DeleteUserFailed(principal.Identity.Name.Split('\\')[1].ToString(), username,"not authorized");
                Console.WriteLine("DeleteUser() failed for user {0}.", principal.Identity.Name);
                return allowed;
        }

        public bool EditUser(User user)
        {
            bool allowed = false;

            CustomPrincipal principal = Thread.CurrentPrincipal as CustomPrincipal;
            if (principal.IsInRole("BetAdmin"))
            {
              //  Audit.AuthorizationSuccess(principal.Identity.Name.Split('\\')[1].ToString(), "editUser");
                proxy.EditUser(user);
              //  Audit.EditUser(principal.Identity.Name.Split('\\')[1].ToString(), user.Username.ToString());
                allowed = true;
            }
            else
            {
               // Audit.AuthorizationFailed(principal.Identity.Name.Split('\\')[1].ToString(), "EditUser","not authorized");
               // Audit.EditUserFailed(principal.Identity.Name.Split('\\')[1].ToString(), user.Username.ToString(),"not authorized");
                Console.WriteLine("EditUser() failed for user {0}.", principal.Identity.Name);
            }
            return allowed;
        }

        public bool SendPort(string username, int port)
        {
            return proxy.SendPort(username, port);
        }

        public bool SendTicket(Ticket ticket, string username)
        {
            bool allowed = false;

            CustomPrincipal principal = Thread.CurrentPrincipal as CustomPrincipal;
            if (principal.IsInRole("User"))
            {
              //  Audit.AuthorizationSuccess(principal.Identity.Name.Split('\\')[1].ToString(), "sendticket");
                proxy.SendTicket(ticket, username);
              //  Audit.TicketSent(principal.Identity.Name.Split('\\')[1].ToString());
                allowed = true;
            }
            else
            {
              //  Audit.AuthorizationFailed(principal.Identity.Name.Split('\\')[1].ToString(), "sendticket","not authorized");
              //  Audit.TicketSentFailed(principal.Identity.Name.Split('\\')[1].ToString(), "not authorized");
                Console.WriteLine("SendTicket() failed for user {0}.", principal.Identity.Name);
            }
            return allowed;
        }
    }
}
