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
            EndpointAddress address = new EndpointAddress(new Uri("net.tcp://localhost:" + Helper.bankServicePort + "/BankService"),
                                      new X509CertificateEndpointIdentity(srvCert));

            proxy = new BankServiceProxy(binding, address);
        }

        public bool BankLogin(byte[] usernameBytes, byte[] passwordBytes, byte[] portBytes,byte[] addressBytes)
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


                proxy.BankLogin(encryptedUser, encryptedPassword, encryptedPort, encryptedAddress);

                Audit.AuthenticationSuccess(principal.Identity.Name.Split('\\')[1].ToString());

                Audit.LogIn(principal.Identity.Name.Split('\\')[1].ToString());
                loger.Debug("IP address: {0} - User {1} logged in.", Helper.GetIP(), username);

                allowed = true;
            }

            else
            {
                loger.Debug("IP address: {0} - User {1} not authorized to log in.", Helper.GetIP(), username);
                Audit.AuthorizationFailed(principal.Identity.Name.Split('\\')[1].ToString(), "AddUser", "not authorized");
                Audit.LogInFailed(principal.Identity.Name.Split('\\')[1].ToString(), "not authorized");
            }
            return allowed;
        }

        public bool CheckIfAlive()
        {
            return proxy.CheckIfAlive();
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

                proxy.Deposit(encryptedAccount, encryptedUsername);
                Audit.Deposit(principal.Identity.Name.Split('\\')[1].ToString(), acc.Number.ToString());
                loger.Debug("IP address: {0} - User {1} deposited {2}.", Helper.GetIP(), username, acc.Number);
                allowed = true;
            }
            else
            {
                Audit.AuthorizationFailed(principal.Identity.Name.Split('\\')[1].ToString(), "Deposit", "not authorized");
                Audit.DepositFailed(principal.Identity.Name.Split('\\')[1].ToString(), acc.Number.ToString(), "not authorized");
                loger.Debug("IP address: {0} - User {1} couldn't deposit {2}.", Helper.GetIP(), username, acc.Number);
                Console.WriteLine("Deposit() failed for user {0}.", principal.Identity.Name);          
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

                proxy.CreateAccount(encryptedUser);
                Audit.CreateAccount(principal.Identity.Name.Split('\\')[1].ToString());

                loger.Debug("IP address: {0} - User {1} has been created.", Helper.GetIP(), user.Username);
                allowed = true;
            }
            else
            {
                Audit.AuthorizationFailed(principal.Identity.Name.Split('\\')[1].ToString(), "create account", "not authorized");
                Audit.CreateAccountFailed(principal.Identity.Name.Split('\\')[1].ToString(), "not authorized");

                loger.Debug("IP address: {0} - User {1} couldn't be created.", Helper.GetIP(), user.Username);
                Console.WriteLine("CreateAccount() failed for user {0}.", principal.Identity.Name);
            }

            return allowed;
        }
    }
}
