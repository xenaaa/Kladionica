using CertificateManager;
using Contracts;
using NLog;
using SecurityManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
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
        private static readonly Logger loger = LogManager.GetLogger("Syslog");
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
            string username = (string)Helper.ByteArrayToObject(usernameBytes);
            CustomPrincipal principal = Thread.CurrentPrincipal as CustomPrincipal;
            if (principal.IsInRole("User") || principal.IsInRole("Reader") || principal.IsInRole("BetAdmin"))
            {
               

                byte[] encryptedUser = Helper.EncryptOnIntegration(usernameBytes);
                byte[] encryptedPassword = Helper.EncryptOnIntegration(passwordBytes);
                byte[] encryptedPort = Helper.EncryptOnIntegration(portBytes);
                proxy.BetLogin(encryptedUser, encryptedPassword, encryptedPort);

                Audit.AuthenticationSuccess(principal.Identity.Name.Split('\\')[1].ToString());

                Audit.LogIn(principal.Identity.Name.Split('\\')[1].ToString());
                loger.Debug("IP address: {0} - User {1} has been loged in.", Helper.GetIP(), Helper.ByteArrayToObject(usernameBytes));

                allowed = true;
            }

            else
            {

                Audit.AuthorizationFailed(principal.Identity.Name.Split('\\')[1].ToString(), "AddUser", "not authorized");
                Audit.LogInFailed(principal.Identity.Name.Split('\\')[1].ToString(), "not authorized");


                Audit.AuthorizationFailed(principal.Identity.Name.Split('\\')[1].ToString(), "BetLogin", "not authorized");
                loger.Debug("IP address: {0} - User {1} not authorized to log in.", Helper.GetIP(), username);
            }
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

                loger.Debug("IP address: {0} - User {1} has been added.", Helper.GetIP(),user.Username);

                Audit.AddUser(principal.Identity.Name.Split('\\')[1].ToString(), user.Username.ToString());
                allowed = true;
            }
            else
            {
                Audit.AuthorizationFailed(principal.Identity.Name.Split('\\')[1].ToString(), "AddUser","not authorized");
                Audit.AddUserFailed(principal.Identity.Name.Split('\\')[1].ToString(), user.Username.ToString(),"not authorized");

                loger.Debug("IP address: {0} - User {1} not authorized to add.", Helper.GetIP(), user.Username);

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

                loger.Debug("IP address: {0} - User {1} has been deleted.", Helper.GetIP(), Helper.ByteArrayToObject(usernameBytes));

                allowed = true;
            }
            else
            {
                Audit.AuthorizationFailed(principal.Identity.Name.Split('\\')[1].ToString(), "DeleteUser", "not authorized");
                Audit.DeleteUserFailed(principal.Identity.Name.Split('\\')[1].ToString(), username, "not authorized");

                loger.Debug("IP address: {0} - User is not authorized to delete.", Helper.GetIP());

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

                loger.Debug("IP address: {0} - User {1} has been edited.", Helper.GetIP(), user.Username.ToString());
                allowed = true;
            }
            else
            {
                Audit.AuthorizationFailed(principal.Identity.Name.Split('\\')[1].ToString(), "EditUser", "not authorized");
                Audit.EditUserFailed(principal.Identity.Name.Split('\\')[1].ToString(), user.Username.ToString(), "not authorized");

                loger.Debug("IP address: {0} - User not authorized to edit.", Helper.GetIP());

                Console.WriteLine("EditUser() failed for user {0}.", principal.Identity.Name);
            }
            return allowed;
        }


        public bool SendPort(byte[] usernameBytes, byte[] portBytes, byte[] addressBytes)
        {
            //OperationContext context = OperationContext.Current;
            //MessageProperties properties = context.IncomingMessageProperties;

            //RemoteEndpointMessageProperty endpoint = properties[RemoteEndpointMessageProperty.Name] as RemoteEndpointMessageProperty;

            //string address = string.Empty;

            //if (properties.Keys.Contains(HttpRequestMessageProperty.Name))
            //{
            //    HttpRequestMessageProperty endpointLoadBalancer = properties[HttpRequestMessageProperty.Name] as HttpRequestMessageProperty;
            //    if (endpointLoadBalancer != null && endpointLoadBalancer.Headers["X-Forwarded-For"] != null)
            //        address = endpointLoadBalancer.Headers["X-Forwarded-For"];
            //}
            //if (string.IsNullOrEmpty(address))
            //{
            //    address = endpoint.Address;
            //}

           // string IPP = Helper.GetIP();


            //OperationContext oOperationContext = OperationContext.Current;
            //MessageProperties oMessageProperties = oOperationContext.IncomingMessageProperties;
          
            //RemoteEndpointMessageProperty endpoint = oMessageProperties[RemoteEndpointMessageProperty.Name] as RemoteEndpointMessageProperty;

            //string addressIPv6 = endpoint.Address;
            //int nPort = endpoint.Port;


            //byte[] encryptedAddress = Helper.Encrypt(addressIPv6);//ako je vracena adresa vec zapravo IPv4, moze se dasiti...

            //IPAddress ipAddress = IPAddress.Parse(addressIPv6);
            //IPHostEntry ipHostEntry = Dns.GetHostEntry(ipAddress);
            //foreach (IPAddress address in ipHostEntry.AddressList)
            //{
            //    if (address.AddressFamily == AddressFamily.InterNetwork)
            //        encryptedAddress = Helper.Encrypt(address.ToString());
               
            //}

            byte[] encryptedAddress = Helper.Encrypt(Helper.GetIP());

            byte[] encryptedUsername = Helper.EncryptOnIntegration(usernameBytes);
            byte[] encryptedPort = Helper.EncryptOnIntegration(portBytes);
           // byte[] encryptedAddress = Helper.Encrypt(address);

            return proxy.SendPort(encryptedUsername, encryptedPort, encryptedAddress);
        }


        public bool SendTicket(byte[] ticketBytes, byte[] usernameBytes)
        {
            bool allowed = false;

            string username = (string)Helper.ByteArrayToObject(usernameBytes);

            CustomPrincipal principal = Thread.CurrentPrincipal as CustomPrincipal;
            if (principal.IsInRole("User"))
            {
                Audit.AuthorizationSuccess(principal.Identity.Name.Split('\\')[1].ToString(), "sendticket");

                byte[] encryptedTicket = Helper.EncryptOnIntegration(ticketBytes);
                byte[] encryptedUsername = Helper.EncryptOnIntegration(usernameBytes);

                //OperationContext context = OperationContext.Current;
                //MessageProperties properties = context.IncomingMessageProperties;

                //RemoteEndpointMessageProperty endpoint = properties[RemoteEndpointMessageProperty.Name] as RemoteEndpointMessageProperty;

                //

                //if (properties.Keys.Contains(HttpRequestMessageProperty.Name))
                //{
                //    HttpRequestMessageProperty endpointLoadBalancer = properties[HttpRequestMessageProperty.Name] as HttpRequestMessageProperty;
                //    if (endpointLoadBalancer != null && endpointLoadBalancer.Headers["X-Forwarded-For"] != null)
                //        address = endpointLoadBalancer.Headers["X-Forwarded-For"];
                //}
                //if (string.IsNullOrEmpty(address))
                //{
                //    address = endpoint.Address;
                //}


                //OperationContext oOperationContext = OperationContext.Current;
                //MessageProperties oMessageProperties = oOperationContext.IncomingMessageProperties;

                //RemoteEndpointMessageProperty endpoint = oMessageProperties[RemoteEndpointMessageProperty.Name] as RemoteEndpointMessageProperty;

                //string addressIPv6 = endpoint.Address;
                //int nPort = endpoint.Port;


                //addressIPv4 = addressIPv6;//ako je vracena adresa vec zapravo IPv4, moze se dasiti...


                //byte[] encryptedAddress = Helper.Encrypt(addressIPv6);

                //IPAddress ipAddress = IPAddress.Parse(addressIPv6);
                //IPHostEntry ipHostEntry = Dns.GetHostEntry(ipAddress);
                //foreach (IPAddress address in ipHostEntry.AddressList)
                //{
                //    if (address.AddressFamily == AddressFamily.InterNetwork)
                //        addressIPv4 = address.ToString();

                //}
                string addressIPv4 = Helper.GetIP();

                loger.Debug("User {0} sent ticket from address {1}.", username, addressIPv4);




                proxy.SendTicket(encryptedTicket, encryptedUsername);
                Audit.TicketSent(principal.Identity.Name.Split('\\')[1].ToString());

                loger.Debug("IP address: {0} - User {1} sent ticket.", Helper.GetIP(), username);

                allowed = true;
            }
            else
            {
               Audit.AuthorizationFailed(principal.Identity.Name.Split('\\')[1].ToString(), "sendticket","not authorized");
               Audit.TicketSentFailed(principal.Identity.Name.Split('\\')[1].ToString(), "not authorized");

                loger.Debug("IP address: {0} - User {1} not authorized to send ticket.", Helper.GetIP(), username);

                Console.WriteLine("SendTicket() failed for user {0}.", principal.Identity.Name);
            }
            return allowed;
        }
    }
}
