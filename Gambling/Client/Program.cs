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
            shaHash = SHA512.Create();

            Console.ForegroundColor = ConsoleColor.White;
            NetTcpBinding binding = new NetTcpBinding();

            port = FreeTcpPort();

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
                    BankService(port);
                }

                else if (inputValue == 2)
                {
                    BetService(port);
                }

                else if (inputValue == 3)
                {
                    break;
                }
            }
            // }
            host.Close();
        }


        private static bool MakeTicket(Dictionary<int, Game> bets, int payment)
        {
            WindowsIdentity clientIdentity = WindowsIdentity.GetCurrent();
            Ticket ticket = new Ticket(bets, payment);

            if (!BetDisconnected)
            {

                foreach (var item in bets)
                {
                    if (!ClientHelper.Offers.ContainsKey(item.Key))
                        return false;
                }

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
                    return false;
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

        private static void BankService(int port)
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
            string username;

            if (!BankDisconnected)
            {
                while (true)
                {
                    Console.WriteLine("Enter username:");
                    username = Console.ReadLine();

                    Console.WriteLine("Enter password:");
                    password = Console.ReadLine();

                    if (BankDisconnected)
                        break;

                    if (!BankProxy.BankLogin(Helper.ObjectToByteArray(username), Helper.ObjectToByteArray(GetSha512Hash(shaHash, password)), Helper.ObjectToByteArray(port), Helper.ObjectToByteArray(0)))
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Wrong username or password. Try again");
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

                        Console.WriteLine("Press 1 for creating new account.");
                        Console.WriteLine("Press 2 for report.");
                        Console.WriteLine("Press 3 for deposit.");
                        Console.WriteLine("Press 4 for logout.");
                        inputValue = (int)CheckIfNumber(Console.ReadLine());

                    } while (inputValue != 1 && inputValue != 2 && inputValue != 3 && inputValue != 4);

                    if (inputValue == 1)
                    {
                        if (BankDisconnected)
                            break;

                        //opcija 1 za testiranje
                        //if (!bankProxy.CreateAccount(Helper.ObjectToByteArray(new User("nemanja", GetSha512Hash(shaHash, "nemanja"), "User")), Helper.ObjectToByteArray(port)))
                        //    Console.WriteLine("User already exists");

                        //   opcija 2 za testiranje
                        Console.WriteLine("Enter username: ");
                        string un = Console.ReadLine();
                        Console.WriteLine("Enter password:");
                        password = Console.ReadLine();
                        Console.WriteLine("Enter role:");
                        string role = Console.ReadLine();
                        if (BankDisconnected)
                            break;
                        if (!bankProxy.CreateAccount(Helper.ObjectToByteArray(new User(un, GetSha512Hash(shaHash, password), role)), Helper.ObjectToByteArray(port)))
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("Failed to create user (not authorized/already exists)");
                            Console.ForegroundColor = ConsoleColor.White;
                        }
                    }

                    else if (inputValue == 2)
                    {
                        if (BankDisconnected)
                            break;
                        List<Dictionary<string, int>> dictionaries = bankProxy.Report(port);

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
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("Failed to create report (not authorized)");
                            Console.ForegroundColor = ConsoleColor.White;
                        }
                    }

                    if (inputValue == 3)
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
                        if (!BankProxy.Deposit(Helper.ObjectToByteArray(new Account(amount, accountNumber)), Helper.ObjectToByteArray(username), Helper.ObjectToByteArray(port)))
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("Deposit failed");
                            Console.ForegroundColor = ConsoleColor.White;
                        }
                    }
                    else if (inputValue == 4)
                        break;
                }
            }

            else
                Console.WriteLine("Server is down");
        }



        private static void BetService(int port)
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
            string username = "";

            if (!BetDisconnected)
            {
                while (true)
                {

                    Console.WriteLine("Enter username: ");
                    username = Console.ReadLine();

                    Console.WriteLine("Enter password:");
                    password = Console.ReadLine();

                    if (BankDisconnected)
                        break;

                    if (!BetProxy.BetLogin(Helper.ObjectToByteArray(username), Helper.ObjectToByteArray(GetSha512Hash(shaHash, password)), Helper.ObjectToByteArray(port)))
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Wrong username or password. Try again");
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                    else
                    {
                        BetProxy.SendPort(Helper.ObjectToByteArray(username), Helper.ObjectToByteArray(port), Helper.ObjectToByteArray(0), Helper.ObjectToByteArray(ClientPrintPort));
                        break;
                    }
                }


                Dictionary<int, Game> bets;
                Game g;
                int code;


                while (true)
                {
                    if (BetDisconnected)
                        break;
            
                    do
                    {
                        if (BetDisconnected)
                            break;

                        Console.WriteLine("Press 1 for adding new client.");
                        Console.WriteLine("Press 2 for editing client.");
                        Console.WriteLine("Press 3 for deleting client.");
                        Console.WriteLine("Press 4 for report.");
                        Console.WriteLine("Press 5 for new ticket.");
                        Console.WriteLine("Press 6 for logout.");
                        inputValue = (int)CheckIfNumber(Console.ReadLine());

                    } while (inputValue != 1 && inputValue != 2 && inputValue != 3 && inputValue != 4 && inputValue != 5 && inputValue != 6);



                    if (inputValue == 1)
                    {
                        if (BetDisconnected)
                            break;
                        //opcija 1 za testiranje
                        //if (!BetProxy.AddUser(Helper.ObjectToByteArray(new User("nemanja", GetSha512Hash(shaHash, "nemanja"), "User")), Helper.ObjectToByteArray(port)))
                        //    Console.WriteLine("User already exists");
                        //opcija 2 za testiranje
                        Console.WriteLine("Enter username: ");
                        string un = Console.ReadLine();
                        Console.WriteLine("Enter password:");
                        password = Console.ReadLine();
                        Console.WriteLine("Enter role:");
                        string role = Console.ReadLine();
                        if (BetDisconnected)
                            break;
                        if (!BetProxy.AddUser(Helper.ObjectToByteArray(new User(un, GetSha512Hash(shaHash, password), role)), Helper.ObjectToByteArray(port)))
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("Failed to add user (not authorized/already exists)");
                            Console.ForegroundColor = ConsoleColor.White;
                        }
                    }

                    else if (inputValue == 2)
                    {
                        if (BetDisconnected)
                            break;
                        //ovo za testiranje         
                        //User user = new User("marina", GetSha512Hash(shaHash, "marina"), "User");
                        //user.BetAccount.Amount = 65;

                        //if (BetDisconnected)
                        //    break;
                        //if (!BetProxy.EditUser(Helper.ObjectToByteArray(user), Helper.ObjectToByteArray(port)))
                        //    Console.WriteLine("User doesn't exist");

                        //ili ovo za testiranje
                        Console.WriteLine("Enter username: ");
                        string un = Console.ReadLine();
                        User user2 = new User(un, GetSha512Hash(shaHash, un), "User");
                        user2.BetAccount.Amount = 1000;
                        if (!BetProxy.EditUser(Helper.ObjectToByteArray(user2), Helper.ObjectToByteArray(port)))
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("Failed to edit user (not authorized/already exists)");
                            Console.ForegroundColor = ConsoleColor.White;
                        }
                    }

                    else if (inputValue == 3)
                    {
                        if (BetDisconnected)
                            break;
                        //ovo za testiranje                               
                        //if (!BetProxy.DeleteUser(Helper.ObjectToByteArray("marina"), Helper.ObjectToByteArray(port)))
                        //    Console.WriteLine("User doesn't exist");
                        //ili ovo za testiranje
                        Console.WriteLine("Enter username: ");
                        string un = Console.ReadLine();
                        if (BetDisconnected)
                            break;
                        if (!BetProxy.DeleteUser(Helper.ObjectToByteArray(un), Helper.ObjectToByteArray(port)))
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("Failed to delete user (not authorized/already exists)");
                            Console.ForegroundColor = ConsoleColor.White;

                        }
                    }
                    else if (inputValue == 4)
                    {
                        if (BetDisconnected)
                            break;

                        List<Dictionary<string, int>> dictionaries = BetProxy.Report(port);

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
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("Failed to create report (not authorized)");
                            Console.ForegroundColor = ConsoleColor.White;
                        }
                    }

                    if (inputValue == 5)
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
                                            bets.Add(code, g);
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
                                        if (!MakeTicket(bets, payment))
                                        {
                                            Console.ForegroundColor = ConsoleColor.Red;
                                            Console.WriteLine("Failed to send ticket (not authorized/don't have enough money/game finished)");
                                            Console.ForegroundColor = ConsoleColor.White;
                                        }

                                    }
                                    break;
                                }

                            } while (true);
                            break;
                        }
                    }
                    else if (inputValue == 6)
                    {
                        break;
                    }
                }
            }
            else
                Console.WriteLine("Server is down");
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