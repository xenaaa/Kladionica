using CertificateManager;
using Contracts;
using SecurityManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.ServiceModel.Channels;
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

            X509Certificate2 srvCert = CertManager.GetCertificateFromStorage(StoreName.My, StoreLocation.LocalMachine, srvCertCN);
            EndpointAddress address = new EndpointAddress(new Uri("net.tcp://localhost:"+ Helper.betServicePort + "/BetService"),
                                      new X509CertificateEndpointIdentity(srvCert));

            proxy = new BetServiceProxy(binding, address);
        }

        public bool BetLogin(byte[] usernameBytes, byte[] passwordBytes, byte[] portBytes)
        {

            bool allowed = false;

            CustomPrincipal principal = Thread.CurrentPrincipal as CustomPrincipal;
            if (principal.IsInRole("User") || principal.IsInRole("Reader") || principal.IsInRole("BetAdmin"))
            {
                Audit.AuthenticationSuccess(principal.Identity.Name.Split('\\')[1].ToString());

                byte[] encryptedUser = Helper.EncryptOnIntegration(usernameBytes);
                byte[] encryptedPassword = Helper.EncryptOnIntegration(passwordBytes);
                byte[] encryptedPort = Helper.EncryptOnIntegration(portBytes);
                proxy.BetLogin(encryptedUser, encryptedPassword, encryptedPort);


                allowed = true;
            }

            else
                Audit.AuthorizationFailed(principal.Identity.Name.Split('\\')[1].ToString(), "BetLogin", "not authorized");
            return allowed;
        }

        public bool CheckIfAlive()
        {
            return proxy.CheckIfAlive();
        }


        public bool AddUser(byte[] userBytes)
        {
            bool allowed = false;
            User user = (User)Helper.ByteArrayToObject(userBytes);

            CustomPrincipal principal = Thread.CurrentPrincipal as CustomPrincipal;
            if (principal.IsInRole("BetAdmin"))
            {
                Audit.AuthorizationSuccess(principal.Identity.Name.Split('\\')[1].ToString(), "AddUser");

                byte[] encryptedUser = Helper.EncryptOnIntegration(userBytes);
                proxy.AddUser(encryptedUser);

                Audit.AddUser(principal.Identity.Name.Split('\\')[1].ToString(), user.Username.ToString());
                allowed = true;
            }
            else
            {
                Audit.AuthorizationFailed(principal.Identity.Name.Split('\\')[1].ToString(), "AddUser","not authorized");
                Audit.AddUserFailed(principal.Identity.Name.Split('\\')[1].ToString(), user.Username.ToString(),"not authorized");
                Console.WriteLine("AddUser() failed for user {0}.", principal.Identity.Name);
            }
            return allowed;
        }

        public bool DeleteUser(byte[] usernameBytes)
        {
            bool allowed = false;
            string username = (string)Helper.ByteArrayToObject(usernameBytes);

            CustomPrincipal principal = Thread.CurrentPrincipal as CustomPrincipal;
            if (principal.IsInRole("BetAdmin"))
            {
                Audit.AuthorizationSuccess(principal.Identity.Name.Split('\\')[1].ToString(), "DeleteUser");

                byte[] encryptedUser = Helper.EncryptOnIntegration(usernameBytes);

                proxy.DeleteUser(encryptedUser);
                Audit.DeleteUser(principal.Identity.Name.Split('\\')[1].ToString(), username);
                allowed = true;
            }
            else
            {
                Audit.AuthorizationFailed(principal.Identity.Name.Split('\\')[1].ToString(), "DeleteUser", "not authorized");
                Audit.DeleteUserFailed(principal.Identity.Name.Split('\\')[1].ToString(), username, "not authorized");
                Console.WriteLine("DeleteUser() failed for user {0}.", principal.Identity.Name);
            }
                return allowed;
        }

        public bool EditUser(byte[] userBytes)
        {
            bool allowed = false;
            User user = (User)Helper.ByteArrayToObject(userBytes);

            CustomPrincipal principal = Thread.CurrentPrincipal as CustomPrincipal;
            if (principal.IsInRole("BetAdmin"))
            {
                Audit.AuthorizationSuccess(principal.Identity.Name.Split('\\')[1].ToString(), "editUser");

                byte[] encryptedUser = Helper.EncryptOnIntegration(userBytes);
                proxy.EditUser(encryptedUser);

                Audit.EditUser(principal.Identity.Name.Split('\\')[1].ToString(), user.Username.ToString());
                allowed = true;
            }
            else
            {
                Audit.AuthorizationFailed(principal.Identity.Name.Split('\\')[1].ToString(), "EditUser", "not authorized");
                Audit.EditUserFailed(principal.Identity.Name.Split('\\')[1].ToString(), user.Username.ToString(), "not authorized");
                Console.WriteLine("EditUser() failed for user {0}.", principal.Identity.Name);
            }
            return allowed;
        }


        public bool SendPort(byte[] usernameBytes, byte[] portBytes, byte[] addressBytes)
        {
            OperationContext context = OperationContext.Current;
            MessageProperties properties = context.IncomingMessageProperties;

            RemoteEndpointMessageProperty endpoint = properties[RemoteEndpointMessageProperty.Name] as RemoteEndpointMessageProperty;

            string address = string.Empty;

            if (properties.Keys.Contains(HttpRequestMessageProperty.Name))
            {
                HttpRequestMessageProperty endpointLoadBalancer = properties[HttpRequestMessageProperty.Name] as HttpRequestMessageProperty;
                if (endpointLoadBalancer != null && endpointLoadBalancer.Headers["X-Forwarded-For"] != null)
                    address = endpointLoadBalancer.Headers["X-Forwarded-For"];
            }
            if (string.IsNullOrEmpty(address))
            {
                address = endpoint.Address;
            }


            byte[] encryptedUsername = Helper.EncryptOnIntegration(usernameBytes);
            byte[] encryptedPort = Helper.EncryptOnIntegration(portBytes);
            byte[] encryptedAddress = Helper.Encrypt(address);

            return proxy.SendPort(encryptedUsername, encryptedPort, encryptedAddress);
        }


        public bool SendTicket(byte[] ticketBytes, byte[] usernameBytes)
        {
            bool allowed = false;

            CustomPrincipal principal = Thread.CurrentPrincipal as CustomPrincipal;
            if (principal.IsInRole("User"))
            {
                Audit.AuthorizationSuccess(principal.Identity.Name.Split('\\')[1].ToString(), "sendticket");

                byte[] encryptedTicket = Helper.EncryptOnIntegration(ticketBytes);
                byte[] encryptedUsername = Helper.EncryptOnIntegration(usernameBytes);

                proxy.SendTicket(encryptedTicket, encryptedUsername);
                Audit.TicketSent(principal.Identity.Name.Split('\\')[1].ToString());
                allowed = true;
            }
            else
            {
               Audit.AuthorizationFailed(principal.Identity.Name.Split('\\')[1].ToString(), "sendticket","not authorized");
               Audit.TicketSentFailed(principal.Identity.Name.Split('\\')[1].ToString(), "not authorized");
                Console.WriteLine("SendTicket() failed for user {0}.", principal.Identity.Name);
            }
            return allowed;
        }
    }
}
