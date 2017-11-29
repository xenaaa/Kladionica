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
        ClientProxy clientProxy;

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
            if (!string.IsNullOrEmpty(Helper.BetServerAddress))
            {/*provera jer ce banka pozvati endpoint koji implementira BetService, ali ovde postoji pravljenje novog endpointa koj nama u sustini ne treba u tom trenutku
                da smo pravili novi contract ova provera nam ne bi trebala jer bi postojao novi endpoint......*/
                if (!string.IsNullOrEmpty(IP) && Helper.BetServerAddress.Contains(IP))
                    Helper.BetServerAddress = Helper.BetServerAddress.Replace(IP, "localhost");


                EndpointAddress address = new EndpointAddress(new Uri(Helper.BetServerAddress),
                                        new X509CertificateEndpointIdentity(srvCert));

                proxy = new BetServiceProxy(binding, address);

            }
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

        public bool CheckIfAlive(int port)
        {
              //   int port = Helper.GetPort();
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


        public bool AddUser(byte[] userBytes, byte[] portBytes)
        {
            bool allowed = false;
            User user = (User)Helper.ByteArrayToObject(userBytes);
            int port = (int)Helper.ByteArrayToObject(portBytes);

            CustomPrincipal principal = Thread.CurrentPrincipal as CustomPrincipal;
            if (principal.IsInRole("BetAdmin"))
            {
                Audit.AuthorizationSuccess(principal.Identity.Name.Split('\\')[1].ToString(), "AddUser");

                byte[] encryptedUser = Helper.EncryptOnIntegration(userBytes);
                byte[] encryptedPort = Helper.EncryptOnIntegration(portBytes);

                if (proxy.AddUser(encryptedUser, encryptedPort))
                {
                    Audit.AddUser(principal.Identity.Name.Split('\\')[1].ToString(), user.Username.ToString());
                    loger.Info("IP address: {0} Port: {1} - User {2} is added.", Helper.GetIP(), port, user.Username);
                    allowed = true;
                }
                else
                {
                    Audit.AddUserFailed(principal.Identity.Name.Split('\\')[1].ToString(), user.Username.ToString(), "error");
                    loger.Warn("IP address: {0} Port: {1} - Failed to add user {2}.", Helper.GetIP(), port, user.Username);
                    allowed = false;
                }

            }
            else
            {
                Audit.AuthorizationFailed(principal.Identity.Name.Split('\\')[1].ToString(), "AddUser", "not authorized");
                Audit.AddUserFailed(principal.Identity.Name.Split('\\')[1].ToString(), user.Username.ToString(), "not authorized");
                loger.Warn("IP address: {0} Port: {1} - Failed to add user {2} (not authorized).", Helper.GetIP(), port, user.Username);
                allowed = false;
            }



            return allowed;
        }

        public bool DeleteUser(byte[] usernameBytes, byte[] portBytes)
        {
            bool allowed = false;
            string username = (string)Helper.ByteArrayToObject(usernameBytes);
            int port = (int)Helper.ByteArrayToObject(portBytes);

            CustomPrincipal principal = Thread.CurrentPrincipal as CustomPrincipal;
            if (principal.IsInRole("BetAdmin"))
            {
                Audit.AuthorizationSuccess(principal.Identity.Name.Split('\\')[1].ToString(), "DeleteUser");

                byte[] encryptedUser = Helper.EncryptOnIntegration(usernameBytes);
                byte[] encryptedPort = Helper.EncryptOnIntegration(portBytes);

                if (proxy.DeleteUser(encryptedUser, encryptedPort))
                {
                    Audit.DeleteUser(principal.Identity.Name.Split('\\')[1].ToString(), username);
                    loger.Info("IP address: {0} Port: {1} - User {2} is deleted.", Helper.GetIP(), port, username);
                    allowed = true;
                }

                else
                {
                    Audit.DeleteUserFailed(principal.Identity.Name.Split('\\')[1].ToString(), username, "error");
                    loger.Warn("IP address: {0} Port: {1} - Failed to delete user {2}.", Helper.GetIP(), port, username);
                    allowed = false;
                }

            }
            else
            {
                Audit.AuthorizationFailed(principal.Identity.Name.Split('\\')[1].ToString(), "DeleteUser", "not authorized");
                Audit.DeleteUserFailed(principal.Identity.Name.Split('\\')[1].ToString(), username, "not authorized");
                loger.Warn("IP address: {0} Port: {1} - Failed to delete user {2} (not authorized).", Helper.GetIP(), port, username);
                allowed = false;
            }
            return allowed;
        }

        public bool EditUser(byte[] userBytes, byte[] portBytes)
        {
            bool allowed = false;
            User user = (User)Helper.ByteArrayToObject(userBytes);
            int port = (int)Helper.ByteArrayToObject(portBytes);

            CustomPrincipal principal = Thread.CurrentPrincipal as CustomPrincipal;
            if (principal.IsInRole("BetAdmin"))
            {
                Audit.AuthorizationSuccess(principal.Identity.Name.Split('\\')[1].ToString(), "editUser");

                byte[] encryptedUser = Helper.EncryptOnIntegration(userBytes);
                byte[] encryptedPort = Helper.EncryptOnIntegration(portBytes);

                if (proxy.EditUser(encryptedUser,encryptedPort))
                {
                    Audit.EditUser(principal.Identity.Name.Split('\\')[1].ToString(), user.Username.ToString());
                    loger.Info("IP address: {0} Port: {1} - User {2} is edited.", Helper.GetIP(), port, user.Username.ToString());
                    allowed = true;
                }
                else
                {
                    Audit.EditUserFailed(principal.Identity.Name.Split('\\')[1].ToString(), user.Username.ToString(), "error");
                    loger.Warn("IP address: {0} Port: {1} - Failed to edit user {2}.", Helper.GetIP(), port, user.Username.ToString());
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

        public bool Deposit(byte[] accBytes, byte[] usernameBytes, byte[] port)
        {
            return proxy.Deposit(accBytes, usernameBytes,port);
        }

        public bool GetServiceIP(byte[] AddressStringBytes)//proveriti da li se ovo desilo
        {
            string AddressString = Helper.Decrypt(AddressStringBytes) as string;
            Helper.BankServerAddress = AddressString;
            return true;
        }

        //public bool IntrusionPrevention(byte[] user)
        //{

        //    return proxy.IntrusionPrevention(user);

        //}


        public List<Dictionary<string, int>> Report()
        {
            List<Dictionary<string, int>> returnDictionaries = new List<Dictionary<string, int>>();
            Dictionary<string, int> addresses = new Dictionary<string, int>();
            Dictionary<string, int> users = new Dictionary<string, int>();

            string line;
            System.IO.StreamReader file = new System.IO.StreamReader("ESB_" + DateTime.Now.ToString("yyyy-MM-dd") + ".txt");

            while ((line = file.ReadLine()) != null)
            {
                if (!line.Contains("created") && !line.Contains("Deposit"))
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

            //sortiramo adrese i korisnike
            var sortedAddressDict = from entry in addresses orderby entry.Value descending select entry;

            int counter = 3;
            if (counter > sortedAddressDict.Count())
                counter = sortedAddressDict.Count();

            foreach (var item in sortedAddressDict)
            {
                //    Console.WriteLine(item);
                counter--;
                if (counter == 0)
                    break;
            }

            var sortedUserDict = from entry in users orderby entry.Value descending select entry;

            if (counter > sortedUserDict.Count())
                counter = sortedUserDict.Count();

            foreach (var item in sortedUserDict)
            {
                //  Console.WriteLine(item);
                counter--;
                if (counter == 0)
                    break;
            }

            file.Close();

            Dictionary<string, int> result = sortedAddressDict.ToDictionary(x => x.Key, x => x.Value);
            returnDictionaries.Add(result);

            result = sortedUserDict.ToDictionary(x => x.Key, x => x.Value);
            returnDictionaries.Add(result);

            return returnDictionaries;

        }
    }
}