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
    public class BankService : IBankService
    {
        BankServiceProxy proxy;

        public BankService()
        {
            string srvCertCN = "bankservice";

            NetTcpBinding binding = new NetTcpBinding();
            binding.Security.Transport.ClientCredentialType = TcpClientCredentialType.Certificate;
            //   string address = "net.tcp://localhost:9998/BetService";

            X509Certificate2 srvCert = CertManager.GetCertificateFromStorage(StoreName.My, StoreLocation.LocalMachine, srvCertCN);
            EndpointAddress address = new EndpointAddress(new Uri("net.tcp://localhost:9000/BankService"),
                                      new X509CertificateEndpointIdentity(srvCert));

            proxy = new BankServiceProxy(binding, address);
        }

        public bool BankLogin(string username, string password, int port)
        {
            bool allowed = false;

            CustomPrincipal principal = Thread.CurrentPrincipal as CustomPrincipal;
            if (principal.IsInRole("User") || principal.IsInRole("Reader") || principal.IsInRole("BankAdmin"))
            {
                Audit.AuthenticationSuccess(principal.Identity.Name.Split('\\')[1].ToString());
                proxy.BankLogin(username, password, port);
                allowed = true;
            }

            else
                Console.WriteLine("BankLogin() failed for user {0}.", principal.Identity.Name);
            return allowed;
        }

        public bool CheckIfAlive()
        {
            return proxy.CheckIfAlive();
        }

        public bool Deposit(Account acc, string username)
        {
            bool allowed = false;

            CustomPrincipal principal = Thread.CurrentPrincipal as CustomPrincipal;
            if (principal.IsInRole("User") || principal.IsInRole("Reader"))
            {
                Audit.AuthorizationSuccess(principal.Identity.Name.Split('\\')[1].ToString(), "Deposite");
                proxy.Deposit(acc, username);
                Audit.Deposit(principal.Identity.Name.Split('\\')[1].ToString(), acc.Number.ToString());
                allowed = true;
            }
            else
            {
                Audit.AuthorizationFailed(principal.Identity.Name.Split('\\')[1].ToString(), "Deposit", "not authorized");
                Audit.DepositFailed(principal.Identity.Name.Split('\\')[1].ToString(), acc.Number.ToString(), "not authorized");
                Console.WriteLine("Deposit() failed for user {0}.", principal.Identity.Name);          
            }
            return allowed;
        }

        public bool CreateAccount(User user)
        {
            bool allowed = false;

            CustomPrincipal principal = Thread.CurrentPrincipal as CustomPrincipal;
            if (principal.IsInRole("BankAdmin"))
            {
                Audit.AuthorizationSuccess(principal.Identity.Name.Split('\\')[1].ToString(), "create account");

                proxy.CreateAccount(user);
                Audit.CreateAccount(principal.Identity.Name.Split('\\')[1].ToString());
                allowed = true;
            }
            else
            {
                Audit.AuthorizationFailed(principal.Identity.Name.Split('\\')[1].ToString(), "create account", "not authorized");
                Audit.CreateAccountFailed(principal.Identity.Name.Split('\\')[1].ToString(), "not authorized");
                Console.WriteLine("CreateAccount() failed for user {0}.", principal.Identity.Name);
            }
            
            return allowed;
        }
    }
}
