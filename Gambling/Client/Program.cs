using Contracts;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Security.Principal;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Client
{
    class Program
    {
        static int FreeTcpPort()
        {
            TcpListener l = new TcpListener(IPAddress.Loopback, 0);
            l.Start();
            int port = ((IPEndPoint)l.LocalEndpoint).Port;
            l.Stop();
            return port;
        }
        static int ClientPrintPort;
        static int port;

        private static ClientBetProxy betProxy;
        private static ClientBankProxy bankProxy;
        static bool BetDisconnected;
        static bool BankDisconnected;


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


        public static SHA512 shaHash;
        static void Main(string[] args)
        {
            bool bankAdmin = false;
            bool betAdmin = false;
            shaHash = SHA512.Create();

            //proveriti da lie je jedan vec otvoren


            NetTcpBinding binding = new NetTcpBinding();

            port = FreeTcpPort();

            //Console.WriteLine("Enter port: ");
            //int port = Convert.ToInt32(Console.ReadLine());
            //int port = Convert.ToInt32(args[0]);
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
            Console.WriteLine("\nGROUPS\n");
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
                BankAdmin(clientIdentity, port);
            }
            else if (betAdmin)
            {
                BetAdmin(clientIdentity, port);
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
            host.Close();
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
                    Console.WriteLine("\nPayment: " + ticket.Payment);
                    Console.WriteLine("\nPossible win: " + ticket.CashPrize);
                    Console.WriteLine("\n*********************************************************************************");
                    Console.ForegroundColor = ConsoleColor.White;
                }

                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Your ticket is not sent! (You don't have permission to send ticket or you don't have enough money).");
                    Console.ForegroundColor = ConsoleColor.White;
                }
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("You are disconnected.\n");
                Console.ForegroundColor = ConsoleColor.White;
                return false;
            }
            return true;
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

                    if (!BankProxy.BankLogin(Helper.ObjectToByteArray(clientIdentity.Name.Split('\\')[1]), Helper.ObjectToByteArray(GetSha512Hash(shaHash, password)), Helper.ObjectToByteArray(port), Helper.ObjectToByteArray(0)))
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Wrong password. Try again");
                        Console.ForegroundColor = ConsoleColor.White;
                    }
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
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("Deposit failed");
                            Console.ForegroundColor = ConsoleColor.White;
                        }
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

                    if (!BetProxy.BetLogin(Helper.ObjectToByteArray(clientIdentity.Name.Split('\\')[1]), Helper.ObjectToByteArray(GetSha512Hash(shaHash, password)), Helper.ObjectToByteArray(port)))
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Wrong password. Try again");
                        Console.ForegroundColor = ConsoleColor.White;
                    }
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
                        if (BetDisconnected)
                            break;
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
                                        Console.WriteLine("This game finished");
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
                if (!bankProxy.CreateAccount(Helper.ObjectToByteArray(new User("adminBank", GetSha512Hash(shaHash, "admin"), "BankAdmin")), Helper.ObjectToByteArray(port)))
                    Console.WriteLine("User already exists");
                if (!bankProxy.CreateAccount(Helper.ObjectToByteArray(new User("adminBank", GetSha512Hash(shaHash, "admin"), "BankAdmin")), Helper.ObjectToByteArray(port)))
                    Console.WriteLine("User already exists");
                if (!bankProxy.CreateAccount(Helper.ObjectToByteArray(new User("marina", GetSha512Hash(shaHash, "marina"), "User")), Helper.ObjectToByteArray(port)))
                    Console.WriteLine("User already exists");
                if (!bankProxy.CreateAccount(Helper.ObjectToByteArray(new User("bojan", GetSha512Hash(shaHash, "bojan"), "User")), Helper.ObjectToByteArray(port)))
                    Console.WriteLine("User already exists");
                if (!bankProxy.CreateAccount(Helper.ObjectToByteArray(new User("david", GetSha512Hash(shaHash, "david"), "User")), Helper.ObjectToByteArray(port)))
                    Console.WriteLine("User already exists");
                if (!bankProxy.CreateAccount(Helper.ObjectToByteArray(new User("nicpa", GetSha512Hash(shaHash, "nicpa"), "User")), Helper.ObjectToByteArray(port)))
                    Console.WriteLine("User already exists");
                if (!bankProxy.CreateAccount(Helper.ObjectToByteArray(new User("djole", GetSha512Hash(shaHash, "djole"), "Reader")), Helper.ObjectToByteArray(port)))
                    Console.WriteLine("User already exists");

                Console.WriteLine("Your username is: " + clientIdentity.Name.Split('\\')[1]);
                while (true)
                {
                    Console.WriteLine("Enter password:");

                    if (BankDisconnected)
                        break;

                    password = Console.ReadLine();
                    if (!BankProxy.BankLogin(Helper.ObjectToByteArray(clientIdentity.Name.Split('\\')[1]), Helper.ObjectToByteArray(GetSha512Hash(shaHash, password)), Helper.ObjectToByteArray(port), Helper.ObjectToByteArray(0)))
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Wrong password. Try again");
                        Console.ForegroundColor = ConsoleColor.White;
                    }
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

                        if (BankDisconnected)
                            break;

                        //opcija 1 za testiranje
                        if (!bankProxy.CreateAccount(Helper.ObjectToByteArray(new User("nemanja", GetSha512Hash(shaHash, "nemanja"), "User")), Helper.ObjectToByteArray(port)))
                            Console.WriteLine("User already exists");

                        //opcija 2 za testiranje
                        //Console.WriteLine("Enter username: ");
                        //string username = Console.ReadLine();
                        //Console.WriteLine("Enter password:");
                        //password = Console.ReadLine();
                        //Console.WriteLine("Enter role:");
                        //string role = Console.ReadLine();
                        //if (!bankProxy.CreateAccount(Helper.ObjectToByteArray(new User(username, GetSha512Hash(shaHash, password), role))))
                        //    Console.WriteLine("User already exists");

                    }

                    else if (inputValue == 2)
                    {
                        //ovo za testiranje
                        if (BankDisconnected)
                            break;
                        if (!bankProxy.Deposit(Helper.ObjectToByteArray(new Account(4, 12)), Helper.ObjectToByteArray(clientIdentity.Name.Split('\\')[1]), Helper.ObjectToByteArray(port)))
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("Deposit failed");
                            Console.ForegroundColor = ConsoleColor.White;
                        }
                        if (BankDisconnected)
                            break;
                        if (!bankProxy.Deposit(Helper.ObjectToByteArray(new Account(4, 15)), Helper.ObjectToByteArray(clientIdentity.Name.Split('\\')[1]), Helper.ObjectToByteArray(port)))
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("Deposit failed");
                            Console.ForegroundColor = ConsoleColor.White;
                        }

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
                        if (!bankProxy.Deposit(Helper.ObjectToByteArray(new Account(amount, accountNumber)), Helper.ObjectToByteArray(clientIdentity.Name.Split('\\')[1]), Helper.ObjectToByteArray(port)))
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("Deposit failed");
                            Console.ForegroundColor = ConsoleColor.White;
                        }
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
                if (!BetProxy.AddUser(Helper.ObjectToByteArray(new User("adminBet", GetSha512Hash(shaHash, "admin"), "BetAdmin")), Helper.ObjectToByteArray(port)))
                    Console.WriteLine("User already exists");
                if (!BetProxy.AddUser(Helper.ObjectToByteArray(new User("marina", GetSha512Hash(shaHash, "marina"), "User")), Helper.ObjectToByteArray(port)))
                    Console.WriteLine("User already exists");
                if (!BetProxy.AddUser(Helper.ObjectToByteArray(new User("bojan", GetSha512Hash(shaHash, "bojan"), "User")), Helper.ObjectToByteArray(port)))
                    Console.WriteLine("User already exists");
                if (!BetProxy.AddUser(Helper.ObjectToByteArray(new User("david", GetSha512Hash(shaHash, "david"), "User")), Helper.ObjectToByteArray(port)))
                    Console.WriteLine("User already exists");
                if (!BetProxy.AddUser(Helper.ObjectToByteArray(new User("nicpa", GetSha512Hash(shaHash, "nicpa"), "User")), Helper.ObjectToByteArray(port)))
                    Console.WriteLine("User already exists");
                if (!BetProxy.AddUser(Helper.ObjectToByteArray(new User("djole", GetSha512Hash(shaHash, "djole"), "Reader")), Helper.ObjectToByteArray(port)))
                    Console.WriteLine("User already exists");

                BetProxy.SendPort(Helper.ObjectToByteArray("adminBet"), Helper.ObjectToByteArray(port), Helper.ObjectToByteArray(0), Helper.ObjectToByteArray(ClientPrintPort));

                Console.WriteLine("Your username is: " + clientIdentity.Name.Split('\\')[1]);

                while (true)
                {
                    if (BetDisconnected)
                        break;

                    Console.WriteLine("Enter password:");
                    password = Console.ReadLine();

                    if (!BetProxy.BetLogin(Helper.ObjectToByteArray(clientIdentity.Name.Split('\\')[1]), Helper.ObjectToByteArray(GetSha512Hash(shaHash, password)), Helper.ObjectToByteArray(port)))
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Wrong password. Try again");
                        Console.ForegroundColor = ConsoleColor.White;
                    }
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
                        if (BetDisconnected)
                            break;
                        //opcija 1 za testiranje
                        if (!BetProxy.AddUser(Helper.ObjectToByteArray(new User("nemanja", GetSha512Hash(shaHash, "nemanja"), "User")), Helper.ObjectToByteArray(port)))
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
                        User user = new User("marina", GetSha512Hash(shaHash, "marina"), "User");
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
                        if (BetDisconnected)
                            break;
                        //ovo za testiranje                               
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

                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("You are disconnected.");
                    Console.ForegroundColor = ConsoleColor.White;
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
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("You are disconnected.");
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
                }
                Thread.Sleep(3000);
            }
        }

        static string GetSha512Hash(SHA512 shaHash, string input)
        {
            // Convert the input string to a byte array and compute the hash.
            byte[] data = shaHash.ComputeHash(Encoding.UTF8.GetBytes(input));

            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string.
            return sBuilder.ToString();
        }
    }
}