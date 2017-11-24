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

namespace IntegrationPlatform
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

            string IP = string.Empty;
            var hostIP = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in hostIP.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    IP = ip.ToString();
                }
            }

            if (!string.IsNullOrEmpty(IP) && Helper.BetServerAddress.Contains(IP))
                Helper.BetServerAddress = Helper.BetServerAddress.Replace(IP, "localhost");

            EndpointAddress address = new EndpointAddress(new Uri(Helper.BetServerAddress),
                                    new X509CertificateEndpointIdentity(srvCert));

            proxy = new BetServiceProxy(binding, address);
        }

        public bool BetLogin(byte[] usernameBytes, byte[] passwordBytes, byte[] portBytes)
        {

            bool allowed = false;
            string username = (string)Helper.ByteArrayToObject(usernameBytes);
            int port = (int)Helper.ByteArrayToObject(portBytes);

            CustomPrincipal principal = Thread.CurrentPrincipal as CustomPrincipal;
            if (principal.IsInRole("User") || principal.IsInRole("Reader") || principal.IsInRole("BetAdmin"))
            {
                byte[] encryptedUser = Helper.EncryptOnIntegration(usernameBytes);
                byte[] encryptedPassword = Helper.EncryptOnIntegration(passwordBytes);
                byte[] encryptedPort = Helper.EncryptOnIntegration(portBytes);

                if (proxy.BetLogin(encryptedUser, encryptedPassword, encryptedPort))
                {
                    Audit.AuthenticationSuccess(principal.Identity.Name.Split('\\')[1].ToString());
                    Audit.LogIn(principal.Identity.Name.Split('\\')[1].ToString());
                    loger.Info("IP address: {0} Port: {1} - Bet login successful.", Helper.GetIP(), port);
                    allowed = true;
                }
                else
                {
                    Audit.LogInFailed(principal.Identity.Name.Split('\\')[1].ToString(), "wrong password");
                    loger.Warn("IP address: {0} Port: {1} - Bet login failed.", Helper.GetIP(), port);
                    allowed = false;
                }
            }

            else
            {
                Audit.AuthorizationFailed(principal.Identity.Name.Split('\\')[1].ToString(), "AddUser", "not authorized");
                Audit.LogInFailed(principal.Identity.Name.Split('\\')[1].ToString(), "not authorized");
                Audit.AuthorizationFailed(principal.Identity.Name.Split('\\')[1].ToString(), "BetLogin", "not authorized");
                loger.Warn("IP address: {0} Port: {1} -Bet login failed (not authorized).", Helper.GetIP(), port);
                allowed = false;
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

                if (proxy.AddUser(encryptedUser))
                {
                    Audit.AddUser(principal.Identity.Name.Split('\\')[1].ToString(), user.Username.ToString());
                    loger.Info("IP address: {0} Port: {1} - User {2} is added.", Helper.GetIP(), user.Port, user.Username);
                    allowed = true;
                }
                else
                {
                    Audit.AddUserFailed(principal.Identity.Name.Split('\\')[1].ToString(), user.Username.ToString(), "error");
                    loger.Warn("IP address: {0} Port: {1} - Failed to add user {2}.", Helper.GetIP(), user.Port, user.Username);
                    allowed = false;
                }
            }
            else
            {
                Audit.AuthorizationFailed(principal.Identity.Name.Split('\\')[1].ToString(), "AddUser", "not authorized");
                Audit.AddUserFailed(principal.Identity.Name.Split('\\')[1].ToString(), user.Username.ToString(), "not authorized");
                loger.Warn("IP address: {0} Port: {1} - Failed to add user {2} (not authorized).", Helper.GetIP(), user.Port, user.Username);
                allowed = false;
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

                if (proxy.DeleteUser(encryptedUser))
                {
                    Audit.DeleteUser(principal.Identity.Name.Split('\\')[1].ToString(), username);
                    loger.Info("IP address: {0} Port: {1} - User {2} is deleted.", Helper.GetIP(), Helper.GetPort(), username);
                    allowed = true;
                }

                else
                {
                    Audit.DeleteUserFailed(principal.Identity.Name.Split('\\')[1].ToString(), username, "error");
                    loger.Warn("IP address: {0} Port: {1} - Failed to delete user {2}.", Helper.GetIP(), Helper.GetPort(), username);
                    allowed = false;
                }
            }
            else
            {
                Audit.AuthorizationFailed(principal.Identity.Name.Split('\\')[1].ToString(), "DeleteUser", "not authorized");
                Audit.DeleteUserFailed(principal.Identity.Name.Split('\\')[1].ToString(), username, "not authorized");
                loger.Warn("IP address: {0} Port: {1} - Failed to delete user {2} (not authorized).", Helper.GetIP(), Helper.GetPort(), username);
                allowed = false;
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

                if (proxy.EditUser(encryptedUser))
                {
                    Audit.EditUser(principal.Identity.Name.Split('\\')[1].ToString(), user.Username.ToString());
                    loger.Info("IP address: {0} Port: {1} - User {2} is edited.", Helper.GetIP(), user.Port, user.Username.ToString());
                    allowed = true;
                }
                else
                {
                    Audit.EditUserFailed(principal.Identity.Name.Split('\\')[1].ToString(), user.Username.ToString(), "error");
                    loger.Warn("IP address: {0} Port: {1} - Failed to edit user {2}.", Helper.GetIP(), user.Port, user.Username.ToString());
                    allowed = false;
                }
            }
            else
            {
                Audit.AuthorizationFailed(principal.Identity.Name.Split('\\')[1].ToString(), "EditUser", "not authorized");
                Audit.EditUserFailed(principal.Identity.Name.Split('\\')[1].ToString(), user.Username.ToString(), "not authorized");
                loger.Warn("IP address: {0} Port: {1} - Failed to edit user {2} (not authorized).", Helper.GetIP(), user.Port, user.Username.ToString());
                allowed = false;
            }
            return allowed;

        }


        public bool SendPort(byte[] usernameBytes, byte[] portBytes, byte[] addressBytes, byte[] printPortBytes)
        {
            byte[] encryptedUsername = Helper.EncryptOnIntegration(usernameBytes);
            byte[] encryptedPort = Helper.EncryptOnIntegration(portBytes);
            byte[] encryptedAddress = Helper.Encrypt(Helper.GetIP());
            byte[] encryptedprintPort = Helper.EncryptOnIntegration(printPortBytes);

            return proxy.SendPort(encryptedUsername, encryptedPort, encryptedAddress, encryptedprintPort);
        }


        public bool SendTicket(byte[] ticketBytes, byte[] usernameBytes, byte[] portBytes)
        {
            bool allowed = false;

            string username = (string)Helper.ByteArrayToObject(usernameBytes);

            int port = (int)Helper.ByteArrayToObject(portBytes);

            CustomPrincipal principal = Thread.CurrentPrincipal as CustomPrincipal;
            if (principal.IsInRole("User"))
            {
                Audit.AuthorizationSuccess(principal.Identity.Name.Split('\\')[1].ToString(), "sendticket");

                byte[] encryptedTicket = Helper.EncryptOnIntegration(ticketBytes);
                byte[] encryptedUsername = Helper.EncryptOnIntegration(usernameBytes);
                byte[] encryptedPort = Helper.EncryptOnIntegration(portBytes);

                string addressIPv4 = Helper.GetIP();

                if (proxy.SendTicket(encryptedTicket, encryptedUsername, encryptedPort))
                {
                    Audit.TicketSent(principal.Identity.Name.Split('\\')[1].ToString());
                    loger.Info("IP address: {0} Port: {1} - Ticket sent.", Helper.GetIP(), port);
                    allowed = true;
                }
                else
                {
                    Audit.TicketSentFailed(principal.Identity.Name.Split('\\')[1].ToString(), "not enough money");
                    loger.Warn("IP address: {0} Port: {1} - Failed to send ticket.", Helper.GetIP(), port);
                    allowed = false;
                }
            }
            else
            {
                Audit.AuthorizationFailed(principal.Identity.Name.Split('\\')[1].ToString(), "sendticket", "not authorized");
                Audit.TicketSentFailed(principal.Identity.Name.Split('\\')[1].ToString(), "not authorized");
                loger.Warn("IP address: {0} Port: {1} - Failed to send ticket (not authorized).", Helper.GetIP(), port);
                allowed = false;
            }
            return allowed;
        }

        public bool Deposit(byte[] accBytes, byte[] usernameBytes)
        {

            return proxy.Deposit(accBytes, usernameBytes);

        }

        public bool GetServiceIP(byte[] AddressStringBytes)//proveriti da li se ovo desilo
        {
            string AddressString = Helper.Decrypt(AddressStringBytes) as string;
            Helper.BankServerAddress = AddressString;
            return true;
        }

        public bool IntrusionPrevention(byte[] user)
        {
            return proxy.IntrusionPrevention(user);
        }
    }
}
