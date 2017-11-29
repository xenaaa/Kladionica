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
    public class BankService : IBankServiceIntegration
    {
        private static readonly Logger loger = LogManager.GetLogger("Syslog");
        BankServiceProxy proxy;
        ClientProxy clientProxy;

        public BankService()
        {
            string srvCertCN = "bankservice";

            NetTcpBinding binding = new NetTcpBinding();
            binding.Security.Transport.ClientCredentialType = TcpClientCredentialType.Certificate;

            X509Certificate2 srvCert = CertManager.GetCertificateFromStorage(StoreName.TrustedPeople, StoreLocation.LocalMachine, srvCertCN);


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
            int port = (int)Helper.ByteArrayToObject(portBytes);

            CustomPrincipal principal = Thread.CurrentPrincipal as CustomPrincipal;
            if (principal.IsInRole("User") || principal.IsInRole("Reader") || principal.IsInRole("BankAdmin"))
            {
                Audit.AuthenticationSuccess(principal.Identity.Name.Split('\\')[1].ToString());

                byte[] encryptedUser = Helper.EncryptOnIntegration(usernameBytes);
                byte[] encryptedPassword = Helper.EncryptOnIntegration(passwordBytes);
                byte[] encryptedAddress = Helper.Encrypt(Helper.GetIP());

                if (proxy.BankLogin(encryptedUser, encryptedPassword, encryptedAddress))
                {

                    Audit.AuthenticationSuccess(principal.Identity.Name.Split('\\')[1].ToString());
                    Audit.LogIn(principal.Identity.Name.Split('\\')[1].ToString());
                    loger.Info("IP address: {0} Port: {1} - Bank login successful.", Helper.GetIP(), port);
                    allowed = true;
                }
                else
                {
                    Audit.LogInFailed(principal.Identity.Name.Split('\\')[1].ToString(), "wrong password");
                    loger.Warn("IP address: {0} Port: {1} - Bank login failed.", Helper.GetIP(), port);
                    allowed = false;
                }
            }

            else
            {
                Audit.AuthorizationFailed(principal.Identity.Name.Split('\\')[1].ToString(), "AddUser", "not authorized");
                Audit.LogInFailed(principal.Identity.Name.Split('\\')[1].ToString(), "not authorized");
                loger.Warn("IP address: {0} Port: {1} - Bank login failed (not authorized).", Helper.GetIP(), port);
                allowed = false;
            }
            return allowed;

        }

        public bool Deposit(byte[] accBytes, byte[] usernameBytes, byte[] portBytes)
        {
            bool allowed = false;


            Account acc = (Account)Helper.ByteArrayToObject(accBytes);
            string username = (string)Helper.ByteArrayToObject(usernameBytes);
            int port = (int)Helper.ByteArrayToObject(portBytes);

            CustomPrincipal principal = Thread.CurrentPrincipal as CustomPrincipal;
            if (principal.IsInRole("User") || principal.IsInRole("Reader"))
            {
                Audit.AuthorizationSuccess(principal.Identity.Name.Split('\\')[1].ToString(), "Deposit");

                byte[] encryptedAccount = Helper.EncryptOnIntegration(accBytes);
                byte[] encryptedUsername = Helper.EncryptOnIntegration(usernameBytes);


                if (proxy.Deposit(encryptedAccount, encryptedUsername))
                {
                    Audit.Deposit(principal.Identity.Name.Split('\\')[1].ToString(), acc.Number.ToString());
                    loger.Info("IP address: {0} Port: {1} - Deposit success.", Helper.GetIP(), port);
                    allowed = true;
                }
                else
                {
                    Audit.DepositFailed(principal.Identity.Name.Split('\\')[1].ToString(), acc.Number.ToString(), "error");
                    loger.Warn("IP address: {0} Port: {1} - Deposit failed.", Helper.GetIP(), port);
                    allowed = false;
                }

            }
            else
            {
                Audit.AuthorizationFailed(principal.Identity.Name.Split('\\')[1].ToString(), "Deposit", "not authorized");
                Audit.DepositFailed(principal.Identity.Name.Split('\\')[1].ToString(), acc.Number.ToString(), "not authorized");
                loger.Warn("IP address: {0} : Port: {1} - Deposit failed (not authorized).", Helper.GetIP(), port);
                allowed = false;
            }
            return allowed;
        }

        public bool CreateAccount(byte[] userBytes, byte[] portBytes)
        {
            bool allowed = false;
            User user = Helper.ByteArrayToObject(userBytes) as User;
            int port = (int)Helper.ByteArrayToObject(portBytes);

            CustomPrincipal principal = Thread.CurrentPrincipal as CustomPrincipal;
            if (principal.IsInRole("BankAdmin"))
            {
                Audit.AuthorizationSuccess(principal.Identity.Name.Split('\\')[1].ToString(), "create account");

                byte[] encryptedUser = Helper.EncryptOnIntegration(userBytes);

                if (proxy.CreateAccount(encryptedUser))
                {
                    Audit.CreateAccount(principal.Identity.Name.Split('\\')[1].ToString());
                    loger.Info("IP address: {0} Port: {1} - User {2} is created.", Helper.GetIP(), port, user.Username);
                    allowed = true;
                }
                else
                {
                    Audit.CreateAccountFailed(principal.Identity.Name.Split('\\')[1].ToString(), "error");
                    loger.Warn("IP address: {0} Port: {1} - Failed to create user {2}.", Helper.GetIP(), port, user.Username);
                    allowed = false;
                }


            }
            else
            {
                Audit.AuthorizationFailed(principal.Identity.Name.Split('\\')[1].ToString(), "create account", "not authorized");
                Audit.CreateAccountFailed(principal.Identity.Name.Split('\\')[1].ToString(), "not authorized");
                loger.Warn("IP address: {0} Port: {1} - Failed to create user {2} (not authorized).", Helper.GetIP(), port, user.Username);
            }



            return allowed;
        }

        public List<Dictionary<string, int>> Report(int port)
        {
            CustomPrincipal principal = Thread.CurrentPrincipal as CustomPrincipal;

            if (principal.IsInRole("BankAdmin"))
            {
                Audit.AuthorizationSuccess(principal.Identity.Name.Split('\\')[1].ToString(), "report");

                List<Dictionary<string, int>> returnDictionaries = new List<Dictionary<string, int>>();
                Dictionary<string, int> addresses = new Dictionary<string, int>();
                Dictionary<string, int> users = new Dictionary<string, int>();

                string line;
                System.IO.StreamReader file = new System.IO.StreamReader("ESB_" + DateTime.Now.ToString("yyyy-MM-dd") + ".txt");

                while ((line = file.ReadLine()) != null)
                {
                    if (line.Contains("Deposit success"))
                    {
                        int first = line.IndexOf("address: ") + "address: ".Length;
                        int last = line.IndexOf(" Port:", first);
                        string address = line.Substring(first, last - first);

                        int first2 = line.IndexOf("\\") + "\\".Length;
                        int last2 = line.IndexOf(" ", first2);
                        string username = line.Substring(first2, last2 - first2);

                        if (username != "adminBet" && username != "adminBank")
                        {
                            if (addresses.ContainsKey(address))
                            {
                                addresses[address]++;
                            }
                            else
                            {
                                addresses.Add(address, 1);
                            }

                            if (users.ContainsKey(username))
                            {
                                users[username]++;
                            }
                            else
                            {
                                users.Add(username, 1);
                            }
                        }
                    }
                }


                var sortedAddressDict = from entry in addresses orderby entry.Value descending select entry;

                int counter = 3;
                if (counter > sortedAddressDict.Count())
                    counter = sortedAddressDict.Count();

                foreach (var item in sortedAddressDict)
                {
                    counter--;
                    if (counter == 0)
                        break;
                }

                var sortedUserDict = from entry in users orderby entry.Value descending select entry;

                if (counter > sortedUserDict.Count())
                    counter = sortedUserDict.Count();

                foreach (var item in sortedUserDict)
                {
                    counter--;
                    if (counter == 0)
                        break;
                }

                file.Close();

                Dictionary<string, int> result = sortedAddressDict.ToDictionary(x => x.Key, x => x.Value);
                returnDictionaries.Add(result);

                result = sortedUserDict.ToDictionary(x => x.Key, x => x.Value);
                returnDictionaries.Add(result);

                loger.Info("IP address: {0} Port: {1} - Failed to create report.", Helper.GetIP(), port);

                return returnDictionaries;
            }
            else
            {
                Audit.AuthorizationFailed(principal.Identity.Name.Split('\\')[1].ToString(), "report", "not authorized");
                loger.Warn("IP address: {0} Port: {1} - Report is created.", Helper.GetIP(), port);
                return null;
            }
        }


        public bool CheckIfAlive(int port)
        {
            string addressIPv4 = Helper.GetIP();

            NetTcpBinding binding = new NetTcpBinding();


            string address = "net.tcp://" + addressIPv4 + ":" + port + "/ClientHelper";

            clientProxy = new ClientProxy(binding, address);


            if (!Program.proxies.ContainsKey(addressIPv4))
            {
                Dictionary<int, ClientProxy> di = new Dictionary<int, ClientProxy>();
                di.Add(port, clientProxy);
                Program.proxies.Add(addressIPv4, di);
            }
            else
            {
                if (!Program.proxies[addressIPv4].ContainsKey(port))
                    Program.proxies[addressIPv4].Add(port, clientProxy);
            }

            return proxy.CheckIfAlive(port);
        }
    }
}