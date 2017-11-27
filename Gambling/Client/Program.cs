using Contracts;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Principal;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Client
{
    class Program
    {
        static int ClientPrintPort;
        static int port;
        static bool BetDisconnected;
        static bool BankDisconnected;

        private static ClientBetProxy betProxy;
        private static ClientBankProxy bankProxy;

        public static ClientBetProxy BetProxy
        {
            get
            {
                return betProxy;
            }

            set
            {
                betProxy = value;
            }
        }

        public static ClientBankProxy BankProxy
        {
            get
            {
                return bankProxy;
            }

            set
            {
                bankProxy = value;
            }
        }

        static int FreeTcpPort()
        {
            TcpListener l = new TcpListener(IPAddress.Loopback, 0);
            l.Start();
            int port = ((IPEndPoint)l.LocalEndpoint).Port;
            l.Stop();
            return port;
        }

        static void Main(string[] args)
        {
            bool bankAdmin = false;
            bool betAdmin = false;

            port = FreeTcpPort();
            Console.WriteLine(port);

            NetTcpBinding binding = new NetTcpBinding();

            string address = "net.tcp://localhost:" + port + "/ClientHelper";

            ServiceHost host = new ServiceHost(typeof(ClientHelper));
            host.AddServiceEndpoint(typeof(IClientHelper), binding, address);
            try
            {
                host.Open();
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: {0}", e.Message);
            }

            Console.WriteLine("Client service is started.");
            Console.WriteLine("Press <enter> to stop service...");


            //listamo grupe kojima korisnik pripada
            NetTcpBinding binding2 = new NetTcpBinding();
            binding2.Security.Mode = SecurityMode.Transport;
            binding2.Security.Transport.ProtectionLevel = System.Net.Security.ProtectionLevel.EncryptAndSign;
            binding2.Security.Transport.ClientCredentialType = TcpClientCredentialType.Windows;

            WindowsIdentity clientIdentity = WindowsIdentity.GetCurrent();
            foreach (IdentityReference group in clientIdentity.Groups)
            {

                SecurityIdentifier sid = (SecurityIdentifier)group.Translate(typeof(SecurityIdentifier));
                var name = sid.Translate(typeof(NTAccount));
                if (name.ToString().Contains("\\User") || name.ToString().Contains("\\Reader"))
                    Console.WriteLine(name.ToString());
                //ako je admin banke dole mu posebne opcije dajemo
                if (name.ToString().Contains("\\BankAdmin"))
                {
                    Console.WriteLine(name.ToString());
                    bankAdmin = true;
                }

                //ako je admin kladionice dole mu posebne opcije dajemo
                if (name.ToString().Contains("\\BetAdmin"))
                {
                    Console.WriteLine(name.ToString());
                    betAdmin = true;
                }
            }

            //ako nije admin banke otvaramo mu poseban prozor za prikaz ponuda i rezultat
            if (!bankAdmin)
            {
                Process[] clientPrint = Process.GetProcessesByName("ClientPrint");
                if (clientPrint.Length == 0)
                {
                    Process p = new Process();
                    string path = Directory.GetCurrentDirectory();
                    path = path.Replace("Client", "ClientPrint");
                    p.StartInfo.WorkingDirectory = @path;
                    p.StartInfo.FileName = @path + @"\ClientPrint.exe";

                    ClientPrintPort = FreeTcpPort();

                    p.StartInfo.Arguments = ClientPrintPort.ToString();
                    p.StartInfo.UseShellExecute = true;

                    p.StartInfo.CreateNoWindow = false;
                    p.Start();
                }
            }

            if (bankAdmin)
            {
             //   while (true)
                {
                    BankAdmin(clientIdentity, port);
                   // Thread.Sleep(3000);
                }
            }
            else if (betAdmin)
            {
             //   while (true)
                {
                    BetAdmin(clientIdentity, port);
                }
            }

            else
            {
                double inputValue = 0;
                while (true)
                {
                    do
                    {
                        Console.WriteLine(" Press 1 - Bank service");
                        Console.WriteLine(" Press 2 - Bet service");
                        Console.WriteLine(" Press 3 for exit.");

                        inputValue = (int)(CheckIfNumber(Console.ReadLine()));

                    } while (inputValue != 1 && inputValue != 2 && inputValue != 3);


                    if (inputValue == 1)
                    {
                        BankService(clientIdentity, port);
                    }

                    else if (inputValue == 2)
                    {
                        BetService(clientIdentity, port);
                    }

                    else if (inputValue == 3)
                    {
                        break;
                    }
                }
            }
            Console.WriteLine("Press any key to exit..");
            Console.ReadLine();
            host.Close();
        }

        private static void BetAdmin(WindowsIdentity clientIdentity, int port)
        {
            NetTcpBinding binding = new NetTcpBinding();
            string address = "net.tcp://" + Helper.integrationHostAddress + ":" + Helper.integrationHostPort + "/BetIntegrationPlatform";

            BetProxy = new ClientBetProxy(binding, address);

            BetDisconnected = false;
            new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                CheckBetConnection();
            }).Start();


            string password;

            if (!BetDisconnected)
            {
                if (!BetProxy.AddUser(Helper.ObjectToByteArray(new User("adminBet", new HashSet<string>() { "admin" }, "BetAdmin")), Helper.ObjectToByteArray(port)))
                    Console.WriteLine("User already exists");

                if (!BetProxy.AddUser(Helper.ObjectToByteArray(new User("marina", new HashSet<string>() { "marina" }, "User")), Helper.ObjectToByteArray(port)))
                    Console.WriteLine("User already exists");

                if (!BetProxy.AddUser(Helper.ObjectToByteArray(new User("bojan", new HashSet<string>() { "bojan" }, "User")), Helper.ObjectToByteArray(port)))
                    Console.WriteLine("User already exists");

                if (!BetProxy.AddUser(Helper.ObjectToByteArray(new User("david", new HashSet<string>() { "david" }, "User")), Helper.ObjectToByteArray(port)))
                    Console.WriteLine("User already exists");

                if (!BetProxy.AddUser(Helper.ObjectToByteArray(new User("nicpa", new HashSet<string>() { "nicpa" }, "User")), Helper.ObjectToByteArray(port)))
                    Console.WriteLine("User already exists");

                if (!BetProxy.AddUser(Helper.ObjectToByteArray(new User("djole", new HashSet<string>() { "djole" }, "Reader")), Helper.ObjectToByteArray(port)))
                    Console.WriteLine("User already exists");

                BetProxy.SendPort(Helper.ObjectToByteArray("adminBet"), Helper.ObjectToByteArray(port), Helper.ObjectToByteArray(0), Helper.ObjectToByteArray(ClientPrintPort));

                Console.WriteLine("Your username is: " + clientIdentity.Name.Split('\\')[1]);

                while (true)
                {
                    if (BetDisconnected)
                        break;

                    Console.WriteLine("Enter password:");
                    password = Console.ReadLine();

                    if (!BetProxy.BetLogin(Helper.ObjectToByteArray(clientIdentity.Name.Split('\\')[1]), Helper.ObjectToByteArray(new HashSet<string>() { password }), Helper.ObjectToByteArray(port)))
                        Console.WriteLine("Wrong password. Try again");
                    else
                        break;

                }

                double inputValue = 0;
                while (true)
                {
                    if (BetDisconnected)
                        break;

                    do
                    {
                        if (BetDisconnected)
                            break;
                        Console.WriteLine("\n*******BET ADMIN OPTIONS*******\n");
                        Console.WriteLine("Press 1 for adding new client.");
                        Console.WriteLine("Press 2 for editing client.");
                        Console.WriteLine("Press 3 for deleting client.");
                        Console.WriteLine("Press 4 for report.");
                        Console.WriteLine("Press 5 for exit.");
                        inputValue = CheckIfNumber(Console.ReadLine());

                    } while (inputValue != 1 && inputValue != 2 && inputValue != 3 && inputValue != 4 && inputValue != 5);

                    if (inputValue == 1)
                    {
                        //opcija 1 za testiranj
                        if (BetDisconnected)
                            break;

                        if (!BetProxy.AddUser(Helper.ObjectToByteArray(new User("nemanja", new HashSet<string>() { "nemanja" }, "User")), Helper.ObjectToByteArray(port)))
                            Console.WriteLine("User already exists");

                        //opcija 2 za testiranje
                        //Console.WriteLine("Enter username: ");
                        //string username = Console.ReadLine();
                        //Console.WriteLine("Enter password:");
                        //password = Console.ReadLine();
                        //Console.WriteLine("Enter role:");
                        //string role = Console.ReadLine();
                        // if (!BetProxy.AddUser(Helper.ObjectToByteArray(new User(username, new HashSet<string>() { password }, role))))
                        //    Console.WriteLine("User already exists");

                    }

                    else if (inputValue == 2)
                    {
                        //ovo za testiranje         
                        User user = new User("marina", new HashSet<string>() { "marina" }, "User");
                        user.BetAccount.Amount = 65;

                        if (BetDisconnected)
                            break;
                        if (!BetProxy.EditUser(Helper.ObjectToByteArray(user), Helper.ObjectToByteArray(port)))
                            Console.WriteLine("User doesn't exist");


                        //ili ovo za testiranje
                        //Console.WriteLine("Enter username: ");
                        //string username = Console.ReadLine();
                        //User user2 = new User(username, username, "User");
                        //user2.BetAccount.Amount = 1000;
                        //if(!BetProxy.EditUser(user2));
                        //Console.WriteLine("User doesn't exist");
                    }
                    else if (inputValue == 3)
                    {
                        //ovo za testiranje   
                        if (BetDisconnected)
                            break;

                        if (!BetProxy.DeleteUser(Helper.ObjectToByteArray("marina"), Helper.ObjectToByteArray(port)))
                            Console.WriteLine("User doesn't exist");

                        //ili ovo za testiranje
                        //Console.WriteLine("Enter username: ");
                        //string username = Console.ReadLine();
                        //if(!BetProxy.DeleteUser(username));
                        //Console.WriteLine("User doesn't exist");
                    }
                    else if (inputValue == 4)
                    {
                        if (BetDisconnected)
                            break;

                        List<Dictionary<string, int>> dictionaries = BetProxy.Report();

                        if (dictionaries != null)
                        {
                            if (File.Exists("report.txt"))
                            {
                                File.WriteAllText("report.txt", string.Empty);
                            }
                            else
                            {
                                File.Create("report.txt");
                            }

                            using (var sw = new StreamWriter("report.txt", true))
                            {
                                sw.WriteLine("ADDRESSES\n");
                                foreach (var item in dictionaries[0])
                                {
                                    sw.WriteLine(item);
                                }

                                sw.WriteLine("USERS\n");
                                foreach (var item in dictionaries[1])
                                {
                                    sw.WriteLine(item);
                                }

                                sw.Close();
                            }
                        }
                    }

                    else if (inputValue == 5)
                    {
                        break;
                    }
                }
            }
        }


        private static void BankAdmin(WindowsIdentity clientIdentity, int port)
        {
            NetTcpBinding binding = new NetTcpBinding();
            string address = "net.tcp://" + Helper.integrationHostAddress + ":" + Helper.integrationHostPort + "/BankIntegrationPlatform";

            bankProxy = new ClientBankProxy(binding, address);

            BankDisconnected = false;
            new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                CheckBankConnection();
            }).Start();


            string password;
            if (!BankDisconnected)
            {
                if (!bankProxy.CreateAccount(Helper.ObjectToByteArray(new User("adminBank", new HashSet<string>() { "admin" }, "BankAdmin")), Helper.ObjectToByteArray(port)))
                    Console.WriteLine("User already exists");

                if (!bankProxy.CreateAccount(Helper.ObjectToByteArray(new User("marina", new HashSet<string>() { "marina" }, "User")), Helper.ObjectToByteArray(port)))
                    Console.WriteLine("User already exists");

                if (!bankProxy.CreateAccount(Helper.ObjectToByteArray(new User("bojan", new HashSet<string>() { "bojan" }, "User")), Helper.ObjectToByteArray(port)))
                    Console.WriteLine("User already exists");

                if (!bankProxy.CreateAccount(Helper.ObjectToByteArray(new User("david", new HashSet<string>() { "david" }, "User")), Helper.ObjectToByteArray(port)))
                    Console.WriteLine("User already exists");

                if (!bankProxy.CreateAccount(Helper.ObjectToByteArray(new User("nicpa", new HashSet<string>() { "nicpa" }, "User")), Helper.ObjectToByteArray(port)))
                    Console.WriteLine("User already exists");

                if (!bankProxy.CreateAccount(Helper.ObjectToByteArray(new User("djole", new HashSet<string>() { "djole" }, "Reader")), Helper.ObjectToByteArray(port)))
                    Console.WriteLine("User already exists");

                Console.WriteLine("Your username is: " + clientIdentity.Name.Split('\\')[1]);
                while (true)
                {
                    Console.WriteLine("Enter password:");

                    if (BankDisconnected)
                        break;

                    password = Console.ReadLine();
                    if (!bankProxy.BankLogin(Helper.ObjectToByteArray(clientIdentity.Name.Split('\\')[1]), Helper.ObjectToByteArray(new HashSet<string>() { password }), Helper.ObjectToByteArray(port), Helper.ObjectToByteArray(0)))
                        Console.WriteLine("Wrong password. Try again");
                    else
                        break;
                }


                double inputValue = 0;
                while (true)
                {
                    if (BankDisconnected)
                        break;

                    do
                    {
                        if (BankDisconnected)
                            break;
                        Console.WriteLine("\n*****BANK ADMIN OPTIONS******\n");
                        Console.WriteLine("Press 1 for creating new account.");
                        Console.WriteLine("Press 2 for deposit.");
                        Console.WriteLine("Press 3 for report.");
                        Console.WriteLine("Press 4 for exit.");
                        inputValue = CheckIfNumber(Console.ReadLine());

                    } while (inputValue != 1 && inputValue != 2 && inputValue != 3 && inputValue != 4);


                    if (inputValue == 1)
                    {
                        //opcija 1 za testiranje
                        if (BankDisconnected)
                            break;
                        if (!bankProxy.CreateAccount(Helper.ObjectToByteArray(new User("nemanja", new HashSet<string>() { "nemanja" }, "User")), Helper.ObjectToByteArray(port)))
                            Console.WriteLine("User already exists");

                        //opcija 2 za testiranje
                        //Console.WriteLine("Enter username: ");
                        //string username = Console.ReadLine();
                        //Console.WriteLine("Enter password:");
                        //password = Console.ReadLine();
                        //Console.WriteLine("Enter role:");
                        //string role = Console.ReadLine();
                        //if (!bankProxy.CreateAccount(Helper.ObjectToByteArray(new User(username, new HashSet<string>() { password }, role))))
                        //    Console.WriteLine("User already exists");

                    }

                    else if (inputValue == 2)
                    {
                        //ovo za testiranje
                        if (BankDisconnected)
                            break;
                        if (!bankProxy.Deposit(Helper.ObjectToByteArray(new Account(4, 12)), Helper.ObjectToByteArray(clientIdentity.Name.Split('\\')[1]), Helper.ObjectToByteArray(port)))
                            Console.WriteLine("Deposit failed");

                        if (BankDisconnected)
                            break;
                        if (!bankProxy.Deposit(Helper.ObjectToByteArray(new Account(4, 15)), Helper.ObjectToByteArray(clientIdentity.Name.Split('\\')[1]),Helper.ObjectToByteArray(port)))
                            Console.WriteLine("Deposit failed");

                        //ili ovo za testiranje
                        int accountNumber = 0;
                        double amount = 0;
                        do
                        {
                            if (BankDisconnected)
                                break;
                            Console.WriteLine("Enter account number: ");
                            inputValue = CheckIfNumber(Console.ReadLine());
                            if (inputValue != -1)
                            {
                                accountNumber = (int)inputValue;
                            }
                        } while (inputValue == -1);

                        do
                        {
                            if (BankDisconnected)
                                break;
                            Console.WriteLine("Enter amount:");
                            inputValue = CheckIfNumber(Console.ReadLine());
                            if (inputValue != -1)
                            {
                                amount = inputValue;
                            }
                        } while (inputValue == -1);

                        if (BankDisconnected)
                            break;
                        if (!bankProxy.Deposit(Helper.ObjectToByteArray(new Account(amount, accountNumber)), Helper.ObjectToByteArray(clientIdentity.Name.Split('\\')[1]),Helper.ObjectToByteArray(port)))
                            Console.WriteLine("Deposit failed");
                    }
                    else if (inputValue == 3)
                    {
                        if (BankDisconnected)
                            break;
                        List<Dictionary<string, int>> dictionaries = bankProxy.Report();

                        if (dictionaries != null)
                        {

                            if (File.Exists("report.txt"))
                            {
                                File.WriteAllText("report.txt", string.Empty);
                            }
                            else
                            {
                                File.Create("report.txt");
                            }

                            using (var sw = new StreamWriter("report.txt", true))
                            {
                                sw.WriteLine("ADDRESSES\n");
                                foreach (var item in dictionaries[0])
                                {
                                    sw.WriteLine(item);
                                }

                                sw.WriteLine("USERS\n");
                                foreach (var item in dictionaries[1])
                                {
                                    sw.WriteLine(item);
                                }

                                sw.Close();
                            }
                        }
                    }
                    else if (inputValue == 4)
                        break;
                }
            }
        }


        private static void BankService(WindowsIdentity clientIdentity, int port)
        {
            NetTcpBinding binding = new NetTcpBinding();
            string address = "net.tcp://" + Helper.integrationHostAddress + ":" + Helper.integrationHostPort + "/BankIntegrationPlatform";

            BankProxy = new ClientBankProxy(binding, address);

            BankDisconnected = false;
            new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                CheckBankConnection();
            }).Start();


            double inputValue = 0;
            string password;
            if (!BankDisconnected)
            {
                Console.WriteLine("Your username is: " + clientIdentity.Name.Split('\\')[1]);

                while (true)
                {               
                    Console.WriteLine("Enter password:");                
                    if (BankDisconnected)
                        break;

                    password = Console.ReadLine();

                    if (!BankProxy.BankLogin(Helper.ObjectToByteArray(clientIdentity.Name.Split('\\')[1]), Helper.ObjectToByteArray(new HashSet<string>() { password }), Helper.ObjectToByteArray(port), Helper.ObjectToByteArray(0)))
                        Console.WriteLine("Wrong password. Try again");
                    else
                        break;
                }


                while (true)
                {
                    if (BankDisconnected)
                        break;

                    do
                    {
                        if (BankDisconnected)
                            break;

                        Console.WriteLine("Press 1 for deposit.");
                        Console.WriteLine("Press 2 for logout.");
                        inputValue = (int)CheckIfNumber(Console.ReadLine());

                    } while (inputValue != 1 && inputValue != 2);

                    if (inputValue == 1)
                    {
                        //ovo za testiranje
                        //BankProxy.Deposit(Helper.ObjectToByteArray(new Account(4, 12)), Helper.ObjectToByteArray(clientIdentity.Name.Split('\\')[1]));
                        //BankProxy.Deposit(Helper.ObjectToByteArray(new Account(4, 15)), Helper.ObjectToByteArray(clientIdentity.Name.Split('\\')[1]));

                        //ili ovo za testiranje
                        int accountNumber = 0;
                        double amount = 0;
                        do
                        {
                            if (BankDisconnected)
                                break;
                            Console.WriteLine("Enter account number: ");
                            inputValue = CheckIfNumber(Console.ReadLine());
                            if (inputValue != -1)
                            {
                                accountNumber = (int)inputValue;
                            }
                        } while (inputValue == -1);

                        do
                        {
                            if (BankDisconnected)
                                break;
                            Console.WriteLine("Enter amount:");
                            inputValue = CheckIfNumber(Console.ReadLine());
                            if (inputValue != -1)
                            {
                                amount = inputValue;
                            }
                        } while (inputValue == -1);


                        if (BankDisconnected)
                            break;
                        if (!BankProxy.Deposit(Helper.ObjectToByteArray(new Account(amount, accountNumber)), Helper.ObjectToByteArray(clientIdentity.Name.Split('\\')[1]), Helper.ObjectToByteArray(port)))
                            Console.WriteLine("Deposit failed");
                    }
                    else if (inputValue == 2)
                        break;
                }
            }
            else
                Console.WriteLine("Server is down");

        }



        private static void BetService(WindowsIdentity clientIdentity, int port)
        {

            NetTcpBinding binding = new NetTcpBinding();
            string address = "net.tcp://" + Helper.integrationHostAddress + ":" + Helper.integrationHostPort + "/BetIntegrationPlatform";

            BetProxy = new ClientBetProxy(binding, address);

            BetDisconnected = false;
            new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                CheckBetConnection();
            }).Start();


            double inputValue = 0;
            string password;

            if (!BetDisconnected)
            {
                BetProxy.SendPort(Helper.ObjectToByteArray(clientIdentity.Name.Split('\\')[1]), Helper.ObjectToByteArray(port), Helper.ObjectToByteArray(0), Helper.ObjectToByteArray(ClientPrintPort));

                Console.WriteLine("Your username is: " + clientIdentity.Name.Split('\\')[1]);

                while (true)
                {
                    if (BetDisconnected)
                        break;

                    Console.WriteLine("Enter password:");
                    password = Console.ReadLine();

                    if (!BetProxy.BetLogin(Helper.ObjectToByteArray(clientIdentity.Name.Split('\\')[1]), Helper.ObjectToByteArray(new HashSet<string>() { password
    }), Helper.ObjectToByteArray(port)))
                        Console.WriteLine("Wrong password. Try again");
                    else
                        break;
                }

                // proxy.AddUser(new User("lala", "lala", "Admin")); provjera autorizacije


                Dictionary<int, Game> bets;
                Game g;
                int code;

                while (true)
                {
                    if (BetDisconnected)
                        break;

                    while (ClientHelper.Offers.Count < 1)
                        Thread.Sleep(2000);
                    do
                    {
                        Console.WriteLine("Press 1 for new ticket.");
                        Console.WriteLine("Press 2 for logout.");
                        inputValue = (int)CheckIfNumber(Console.ReadLine());

                    } while (inputValue != 1 && inputValue != 2);


                    if (inputValue == 1)
                    {
                        while (true)
                        {
                            if (BetDisconnected)
                                break;

                            bets = new Dictionary<int, Game>();

                            do
                            {
                                if (BetDisconnected)
                                    break;
                                do
                                {
                                    if (BetDisconnected)
                                        break;
                                    Console.WriteLine("\n1.\tAdd tip\n2.\t Exit");
                                    inputValue = CheckIfNumber(Console.ReadLine());
                                } while (inputValue != 1 && inputValue != 2);

                                if (inputValue == 1)
                                {
                                    g = new Game();
                                    do
                                    {
                                        if (BetDisconnected)
                                            break;
                                        Console.WriteLine("\nGame code: ");
                                        inputValue = CheckIfNumber(Console.ReadLine());
                                    } while (inputValue == -1);

                                    code = (int)inputValue;

                                    if (!ClientHelper.Offers.ContainsKey(code))
                                    {
                                        Console.WriteLine("\nThis code doesn't exist!");
                                        continue;
                                    }

                                    do
                                    {
                                        if (BetDisconnected)
                                            break;
                                        Console.WriteLine("\nTip: ");
                                        inputValue = CheckIfNumber(Console.ReadLine());
                                    } while (inputValue == -1);

                                    g.Tip = (int)inputValue;

                                    if (ClientHelper.Offers.ContainsKey(code))
                                    {
                                        g.BetOffer = ClientHelper.Offers[code];

                                        if (!bets.ContainsKey(code))
                                            bets.Add(code, g);//proveriti ako dodaje istu utakmicu
                                    }
                                    else
                                    {
                                        Console.WriteLine("This game already finished");
                                    }

                                }
                                else if (inputValue == 2)
                                {
                                    if (bets.Count > 0)
                                    {
                                        do
                                        {
                                            if (BetDisconnected)
                                                break;
                                            Console.WriteLine("\nPayment: ");
                                            inputValue = CheckIfNumber(Console.ReadLine());
                                        } while (inputValue == -1);

                                        int payment = (int)inputValue;

                                        MakeTicket(bets, payment);
                                    }
                                    break;
                                }

                            } while (true);
                            break;
                        }
                    }
                    else if (inputValue == 2)
                    {
                        break;
                    }
                }
            }
            else
                Console.WriteLine("Server is down");
        }

        private static bool MakeTicket(Dictionary<int, Game> bets, int payment)
        {
            WindowsIdentity clientIdentity = WindowsIdentity.GetCurrent();
            Ticket ticket = new Ticket(bets, payment);

            if (!BetDisconnected)
            {
                if (betProxy.SendTicket(Helper.ObjectToByteArray(ticket), Helper.ObjectToByteArray(clientIdentity.Name.Split('\\')[1]), Helper.ObjectToByteArray(port)))
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("\n************************************TICKET**************************************\n");
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine("-----------------------------------------------------------------------------------");
                    Console.WriteLine("ID |       HOME        |       AWAY        |       ODDS      |       TIP       ");
                    Console.WriteLine("-----------------------------------------------------------------------------------");

                    foreach (KeyValuePair<int, Game> item in ticket.Bets)
                    {
                        Console.WriteLine(String.Format("{0,-10} {1,-10}          {2,-10}             {3,-5}           {4,-5}  ", item.Key, item.Value.BetOffer.Home, item.Value.BetOffer.Away, item.Value.BetOffer.Odds[item.Value.Tip], item.Value.Tip));
                    }

                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("\nPossible win: " + ticket.CashPrize);
                    Console.WriteLine("\n*********************************************************************************");
                    Console.ForegroundColor = ConsoleColor.White;
                }

                else
                {
                    Console.WriteLine("Your ticket is not send! (You don't have permission to send ticket or you don't have enough money.");
                }
            }
            else
            {
                Console.WriteLine("You are disconnected.\n");
                BetDisconnected = true;
                return false;
            }
            return true;
        }

        private static double CheckIfNumber(string input)
        {
            int retValue = -1;
            try
            {
                retValue = Convert.ToInt32(input);
                return retValue;
            }
            catch
            {
                return retValue;
            }
        }

        private static void CheckBetConnection()
        {
            while (true)
            {
                if (!BetProxy.CheckIfAlive(port))
                {
                    BetDisconnected = true;
                    Console.WriteLine("You are disconnected.");
                    break;
                }
                Thread.Sleep(3000);
            }
        }
        private static void CheckBankConnection()
        {
            while (true)
            {
                if (!BankProxy.CheckIfAlive(port))
                {
                    BankDisconnected = true;
                    Console.WriteLine("You are disconnected.");
                    break;
                }
                Thread.Sleep(3000);
            }
        }
    }
}