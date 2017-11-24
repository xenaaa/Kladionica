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
    public class BankService : IBankService
    {
        private static readonly Logger loger = LogManager.GetLogger("Syslog");
        BankServiceProxy proxy;

        public BankService()
        {
            string srvCertCN = "bankservice";

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

            if (!string.IsNullOrEmpty(IP) && Helper.BankServerAddress.Contains(IP))
                Helper.BankServerAddress = Helper.BankServerAddress.Replace(IP, "localhost");



            EndpointAddress address = new EndpointAddress(new Uri(Helper.BankServerAddress),
                                     new X509CertificateEndpointIdentity(srvCert));

            proxy = new BankServiceProxy(binding, address);

        }

        public bool BankLogin(byte[] usernameBytes, byte[] passwordBytes, byte[] portBytes, byte[] addressBytes)
        {

            bool allowed = false;
            string username = (string)Helper.ByteArrayToObject(usernameBytes);
            CustomPrincipal principal = Thread.CurrentPrincipal as CustomPrincipal;
            if (principal.IsInRole("User") || principal.IsInRole("Reader") || principal.IsInRole("BankAdmin"))
            {
                Audit.AuthenticationSuccess(principal.Identity.Name.Split('\\')[1].ToString());

                byte[] encryptedUser = Helper.EncryptOnIntegration(usernameBytes);
                byte[] encryptedPassword = Helper.EncryptOnIntegration(passwordBytes);
                byte[] encryptedPort = Helper.EncryptOnIntegration(portBytes);
                byte[] encryptedAddress = Helper.Encrypt(Helper.GetIP());

                if (proxy.BankLogin(encryptedUser, encryptedPassword, encryptedPort, encryptedAddress))
                {

                    Audit.AuthenticationSuccess(principal.Identity.Name.Split('\\')[1].ToString());
                    Audit.LogIn(principal.Identity.Name.Split('\\')[1].ToString());
                    loger.Info("IP address: {0} Port: {1} - Bank login successful.", Helper.GetIP(), Helper.GetPort());
                    allowed = true;
                }
                else
                {
                    Audit.LogInFailed(principal.Identity.Name.Split('\\')[1].ToString(), "wrong password");
                    loger.Warn("IP address: {0} Port: {1} - Bank login failed.", Helper.GetIP(), Helper.GetPort());
                    allowed = false;
                }

            }

            else
            {
                Audit.AuthorizationFailed(principal.Identity.Name.Split('\\')[1].ToString(), "AddUser", "not authorized");
                Audit.LogInFailed(principal.Identity.Name.Split('\\')[1].ToString(), "not authorized");
                loger.Warn("IP address: {0} Port: {1} - Bank login failed (not authorized).", Helper.GetIP(), Helper.GetPort());
                allowed = false;
            }
            return allowed;

        }

        public bool CheckIfAlive()
        {

            return proxy.CheckIfAlive();

            return false;
        }

        public bool Deposit(byte[] accBytes, byte[] usernameBytes)
        {
            bool allowed = false;

            Account acc = (Account)Helper.ByteArrayToObject(accBytes);
            string username = (string)Helper.ByteArrayToObject(usernameBytes);
            CustomPrincipal principal = Thread.CurrentPrincipal as CustomPrincipal;
            if (principal.IsInRole("User") || principal.IsInRole("Reader"))
            {
                Audit.AuthorizationSuccess(principal.Identity.Name.Split('\\')[1].ToString(), "Deposite");

                byte[] encryptedAccount = Helper.EncryptOnIntegration(accBytes);
                byte[] encryptedUsername = Helper.EncryptOnIntegration(usernameBytes);


                if (proxy.Deposit(encryptedAccount, encryptedUsername))
                {
                    Audit.Deposit(principal.Identity.Name.Split('\\')[1].ToString(), acc.Number.ToString());
                    loger.Info("IP address: {0} Port: {1} - Deposit success.", Helper.GetIP(), Helper.GetPort());
                    allowed = true;
                }
                else
                {
                    Audit.DepositFailed(principal.Identity.Name.Split('\\')[1].ToString(), acc.Number.ToString(), "error");
                    loger.Warn("IP address: {0} Port: {1} - Deposit failed.", Helper.GetIP(), Helper.GetPort());
                    allowed = false;
                }

            }
            else
            {
                Audit.AuthorizationFailed(principal.Identity.Name.Split('\\')[1].ToString(), "Deposit", "not authorized");
                Audit.DepositFailed(principal.Identity.Name.Split('\\')[1].ToString(), acc.Number.ToString(), "not authorized");
                loger.Warn("IP address: {0} : Port: {1} - Deposit failed (not authorized).", Helper.GetIP(), Helper.GetPort());
                allowed = false;
            }
            return allowed;
        }

        public bool CreateAccount(byte[] userBytes)
        {
            bool allowed = false;
            User user = Helper.ByteArrayToObject(userBytes) as User;
            CustomPrincipal principal = Thread.CurrentPrincipal as CustomPrincipal;
            if (principal.IsInRole("BankAdmin"))
            {
                Audit.AuthorizationSuccess(principal.Identity.Name.Split('\\')[1].ToString(), "create account");

                byte[] encryptedUser = Helper.EncryptOnIntegration(userBytes);

                if (proxy.CreateAccount(encryptedUser))
                {
                    Audit.CreateAccount(principal.Identity.Name.Split('\\')[1].ToString());
                    loger.Info("IP address: {0} Port: {1} - User {2} is created.", Helper.GetIP(), Helper.GetPort(), user.Username);
                    allowed = true;
                }
                else
                {
                    Audit.CreateAccountFailed(principal.Identity.Name.Split('\\')[1].ToString(), "error");
                    loger.Warn("IP address: {0} Port: {1} - Failed to create user {2}.", Helper.GetIP(), Helper.GetPort(), user.Username);
                    allowed = false;
                }


            }
            else
            {
                Audit.AuthorizationFailed(principal.Identity.Name.Split('\\')[1].ToString(), "create account", "not authorized");
                Audit.CreateAccountFailed(principal.Identity.Name.Split('\\')[1].ToString(), "not authorized");
                loger.Warn("IP address: {0} Port: {1} - Failed to create user {2} (not authorized).", Helper.GetIP(), Helper.GetPort(), user.Username);
            }



            return allowed;
        }

        public bool IntrusionPrevention(byte[] user)
        {
            byte[] encryptedUser = Helper.EncryptOnIntegration(user);

            return proxy.IntrusionPrevention(user);


        }
    }
}