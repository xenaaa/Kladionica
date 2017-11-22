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


            //proveriti da lie je jedan vec otvoren
            Process[] clientPrint = Process.GetProcessesByName("ClientPrint");
            if (clientPrint.Length == 0)
            {
                Process p = new Process();
                string path = Directory.GetCurrentDirectory();
                path = path.Replace("Client", "ClientPrint");
                p.StartInfo.WorkingDirectory = @path;
                p.StartInfo.FileName = @path + @"\ClientPrint.exe";

                p.StartInfo.UseShellExecute = true;

                p.StartInfo.CreateNoWindow = false;
                p.Start();
            }

            NetTcpBinding binding = new NetTcpBinding();

            int port = FreeTcpPort();

            //Console.WriteLine("Enter port: ");
            //int port = Convert.ToInt32(Console.ReadLine());
            //int port = Convert.ToInt32(args[0]);
            string address = "net.tcp://localhost:" + port + "/ClientHelper";

            ServiceHost host = new ServiceHost(typeof(ClientHelper));
            host.AddServiceEndpoint(typeof(IClientHelper), binding, address);

            host.Open();

            Console.WriteLine("Client service is started.");
            Console.WriteLine("Press <enter> to stop service...");


            //listamo grupe kojima korisnik pripada
            NetTcpBinding binding2 = new NetTcpBinding();
            binding2.Security.Mode = SecurityMode.Transport;
            binding2.Security.Transport.ProtectionLevel = System.Net.Security.ProtectionLevel.EncryptAndSign;
            binding2.Security.Transport.ClientCredentialType = TcpClientCredentialType.Windows;

            WindowsIdentity clientIdentity = WindowsIdentity.GetCurrent();
            Console.WriteLine("\n\nGRUPEEEE\n");
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
                    Console.WriteLine(port.ToString());
                }
            }

            host.Close();
        }


        private static void MakeTicket(ClientBetProxy proxy, Dictionary<int, Game> bets)
        {
            WindowsIdentity clientIdentity = WindowsIdentity.GetCurrent();
            Ticket ticket = new Ticket(bets, 5);
            //  t.Payment = 5;
            // Dictionary<int, Game> bets = new Dictionary<int, Game>();


            /* Game g = new Game();

             g.Odds = ClientHelper.Offers[1001].Odds[1];
             g.Tip = 1;
             g.Won = false;
             bets.Add(1001, g);
             g = new Game();
             g.Odds = ClientHelper.Offers[2002].Odds[0];
             g.Tip = 0;
             g.Won = false;
             bets.Add(2002, g);
             g = new Game();
             g.Odds = ClientHelper.Offers[3002].Odds[2];
             g.Tip = 2;
             g.Won = false;
             bets.Add(3002, g);*/
            // t.Bets = bets;

            if (proxy.SendTicket(Helper.ObjectToByteArray(ticket), Helper.ObjectToByteArray(clientIdentity.Name.Split('\\')[1])))
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
        }

        private static void BankService(WindowsIdentity clientIdentity, int port)
        {
            NetTcpBinding binding = new NetTcpBinding();
            string address = "net.tcp://localhost:"+ Helper.integrationHostPort + "/BankIntegrationPlatform";
            ClientBankProxy proxy = new ClientBankProxy(binding, address);

            double inputValue = 0;
            string password;
            if (proxy.CheckIfAlive())
            {
                do
                {
                    Console.WriteLine("Your username is: " + clientIdentity.Name.Split('\\')[1]);
                    Console.WriteLine("Enter password:");
                    password = Console.ReadLine();
                } while (!proxy.BankLogin(Helper.ObjectToByteArray(clientIdentity.Name.Split('\\')[1]), Helper.ObjectToByteArray(password), Helper.ObjectToByteArray(port), Helper.ObjectToByteArray(0)));

                while (true)
                {
                    do
                    {
                        Console.WriteLine("Press 1 for deposit.");
                        Console.WriteLine("Press 2 for exit.");
                        inputValue = (int)CheckIfNumber(Console.ReadLine());

                    } while (inputValue != 1 && inputValue != 2);

                    if (inputValue == 1)
                    {
                        //ovo za testiranje
                        proxy.Deposit(Helper.ObjectToByteArray(new Account(4, 12)), Helper.ObjectToByteArray(clientIdentity.Name.Split('\\')[1]));
                        proxy.Deposit(Helper.ObjectToByteArray(new Account(4, 15)),Helper.ObjectToByteArray(clientIdentity.Name.Split('\\')[1]));

                        //ili ovo za testiranje
                        int accountNumber = 0;
                        double amount = 0;
                        do
                        {
                            Console.WriteLine("Enter account number: ");
                            inputValue = CheckIfNumber(Console.ReadLine());
                            if (inputValue != -1)
                            {
                                accountNumber = (int)inputValue;
                            }
                        } while (inputValue == -1);

                        do
                        {
                            Console.WriteLine("Enter amount:");
                            inputValue = CheckIfNumber(Console.ReadLine());
                            if (inputValue != -1)
                            {
                                amount = inputValue;
                            }
                        } while (inputValue == -1);

                        proxy.Deposit(Helper.ObjectToByteArray(new Account(amount, accountNumber)), Helper.ObjectToByteArray(clientIdentity.Name.Split('\\')[1]));
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
            string address = "net.tcp://localhost:"+ Helper.integrationHostPort + "/BetIntegrationPlatform";
            ClientBetProxy proxy = new ClientBetProxy(binding, address);

            double inputValue = 0;
            string password;

            if (proxy.CheckIfAlive())
            {
                proxy.SendPort(Helper.ObjectToByteArray(clientIdentity.Name.Split('\\')[1]), Helper.ObjectToByteArray(port),Helper.ObjectToByteArray(0)); //treci parametar zbog intefejsa kasnije citamo adresu

                do
                {
                    Console.WriteLine("Your username is: " + clientIdentity.Name.Split('\\')[1]);
                    Console.WriteLine("Enter password:");
                    password = Console.ReadLine();

                } while (!proxy.BetLogin(Helper.ObjectToByteArray(clientIdentity.Name.Split('\\')[1]), Helper.ObjectToByteArray(password), Helper.ObjectToByteArray(port)));

                // proxy.AddUser(new User("lala", "lala", "Admin")); provjera autorizacije

                Dictionary<int, Game> bets;
                Game g;
                int code;

                while (true)
                {
                    while (ClientHelper.Offers.Count < 1)
                        Thread.Sleep(2000);
                    Console.WriteLine("Press Enter for new ticket");
                    while (true)
                    {
                        if (Console.ReadKey(true).Key == ConsoleKey.Enter && Monitor.TryEnter(ClientHelper.PrintLock))//kada se stisne Enter pravi se novi tiket
                        {
                            bets = new Dictionary<int, Game>();
                            lock (ClientHelper.PrintLock)
                            {
                                do
                                {
                                    do
                                    {
                                        Console.WriteLine("\n1.\tAdd tip\n2.\t Exit");
                                        inputValue = CheckIfNumber(Console.ReadLine());
                                    } while (inputValue != 1 && inputValue != 2);

                                    if (inputValue == 1)
                                    {
                                        g = new Game();
                                        do
                                        {
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
                                            Console.WriteLine("\nTip: ");
                                            inputValue = CheckIfNumber(Console.ReadLine());
                                        } while (inputValue == -1);

                                        g.Tip = (int)inputValue;
                                        g.BetOffer = ClientHelper.Offers[code];
                                        // g.Odds = ClientHelper.Offers[code].Odds[g.Tip];
                                        //  g.Odds = betoffer.Odds[g.Tip];
                                        if (!bets.ContainsKey(code))
                                            bets.Add(code, g);//proveriti ako dodaje istu utakmicu

                                    }
                                    else if (inputValue == 2)
                                    {
                                        if (bets.Count > 0)
                                        {
                                            MakeTicket(proxy, bets);
                                        }
                                        break;
                                    }

                                } while (true);

                                Monitor.Exit(ClientHelper.PrintLock);
                                break;
                            }

                        }
                        else
                        {
                            // Monitor.Exit(ClientHelper.PrintLock);
                            continue;
                        }
                    }

                }
            }
            else
                Console.WriteLine("Server is down");
        }



        private static void BankAdmin(WindowsIdentity clientIdentity, int port)
        {
            NetTcpBinding binding = new NetTcpBinding();
            string address = "net.tcp://localhost:"+ Helper.integrationHostPort + "/BankIntegrationPlatform";

            ClientBankProxy proxy = new ClientBankProxy(binding, address);

            string password;
            if (proxy.CheckIfAlive())
            {
                proxy.CreateAccount(Helper.ObjectToByteArray(new User("admin", "admin", "BankAdmin")));
                proxy.CreateAccount(Helper.ObjectToByteArray(new User("marina", "marina", "User")));
                proxy.CreateAccount(Helper.ObjectToByteArray(new User("bojan", "bojan", "User")));
                proxy.CreateAccount(Helper.ObjectToByteArray(new User("david", "david", "User")));
                proxy.CreateAccount(Helper.ObjectToByteArray(new User("nicpa", "nicpa", "User")));
                proxy.CreateAccount(Helper.ObjectToByteArray(new User("djole", "djole", "Reader")));

                do
                {
                    Console.WriteLine("Your username is: " + clientIdentity.Name.Split('\\')[1]);
                    Console.WriteLine("Enter password:");
                    password = Console.ReadLine();

                } while (!proxy.BankLogin(Helper.ObjectToByteArray(clientIdentity.Name.Split('\\')[1]), Helper.ObjectToByteArray(password), Helper.ObjectToByteArray(port),Helper.ObjectToByteArray(0)));

                double inputValue = 0;
                while (true)
                {
                    do
                    {
                        Console.WriteLine("BANK ADMIN OPTIONS");
                        Console.WriteLine("Press 1 for creating new account.");
                        Console.WriteLine("Press 2 for deposit.");
                        Console.WriteLine("Press 3 for exit.");
                        inputValue = CheckIfNumber(Console.ReadLine());

                    } while (inputValue != 1 && inputValue != 2 && inputValue != 3);


                    if (inputValue == 1)
                    {
                        //opcija 1 za testiranje
                        proxy.CreateAccount(Helper.ObjectToByteArray(new User("nemanja", "nemanja", "User")));

                        //opcija 2 za testiranje
                        Console.WriteLine("Enter username: ");
                        string username = Console.ReadLine();
                        Console.WriteLine("Enter password:");
                        password = Console.ReadLine();
                        Console.WriteLine("Enter role:");
                        string role = Console.ReadLine();
                        proxy.CreateAccount(Helper.ObjectToByteArray(new User(username, password, role)));

                    }

                    else if (inputValue == 2)
                    {
                        //ovo za testiranje
                        proxy.Deposit(Helper.ObjectToByteArray(new Account(4, 12)), Helper.ObjectToByteArray(clientIdentity.Name.Split('\\')[1]));
                        proxy.Deposit(Helper.ObjectToByteArray(new Account(4, 15)), Helper.ObjectToByteArray(clientIdentity.Name.Split('\\')[1]));

                        //ili ovo za testiranje
                        int accountNumber = 0;
                        double amount = 0;
                        do
                        {
                            Console.WriteLine("Enter account number: ");
                            inputValue = CheckIfNumber(Console.ReadLine());
                            if (inputValue != -1)
                            {
                                accountNumber = (int)inputValue;
                            }
                        } while (inputValue == -1);

                        do
                        {
                            Console.WriteLine("Enter amount:");
                            inputValue = CheckIfNumber(Console.ReadLine());
                            if (inputValue != -1)
                            {
                                amount = inputValue;
                            }
                        } while (inputValue == -1);

                        proxy.Deposit(Helper.ObjectToByteArray(new Account(amount, accountNumber)), Helper.ObjectToByteArray(clientIdentity.Name.Split('\\')[1]));
                    }
                    else if (inputValue == 3)
                        break;
                }
            }
        }



        private static void BetAdmin(WindowsIdentity clientIdentity, int port)
        {
            NetTcpBinding binding = new NetTcpBinding();
            string address = "net.tcp://localhost:"+ Helper.integrationHostPort + "/BetIntegrationPlatform";

            ClientBetProxy proxy = new ClientBetProxy(binding, address);
            string password;
            if (proxy.CheckIfAlive())
            {
                proxy.AddUser(Helper.ObjectToByteArray(new User("admin", "admin", "BetAdmin")));
                proxy.AddUser(Helper.ObjectToByteArray(new User("marina", "marina", "User")));
                proxy.AddUser(Helper.ObjectToByteArray(new User("bojan", "bojan", "User")));
                proxy.AddUser(Helper.ObjectToByteArray(new User("david", "david", "User")));
                proxy.AddUser(Helper.ObjectToByteArray(new User("nicpa", "nicpa", "User")));
                proxy.AddUser(Helper.ObjectToByteArray(new User("djole", "djole", "Reader")));

                proxy.SendPort(Helper.ObjectToByteArray("admin"), Helper.ObjectToByteArray(port), Helper.ObjectToByteArray(0)); //treci parametar zbog intefejsa kasnije citamo adresu

                do
                {
                    Console.WriteLine("Your username is: " + clientIdentity.Name.Split('\\')[1]);
                    Console.WriteLine("Enter password:");
                    password = Console.ReadLine();

                } while (!proxy.BetLogin(Helper.ObjectToByteArray(clientIdentity.Name.Split('\\')[1]), Helper.ObjectToByteArray(password), Helper.ObjectToByteArray(port)));


                double inputValue = 0;
                while (true)
                {
                    do
                    {
                        Console.WriteLine("BET ADMIN OPTIONS");
                        Console.WriteLine("Press 1 for adding new client.");
                        Console.WriteLine("Press 2 for editing client.");
                        Console.WriteLine("Press 3 for deleting client.");
                        Console.WriteLine("Press 4 for exit.");
                        inputValue = CheckIfNumber(Console.ReadLine());

                    } while (inputValue != 1 && inputValue != 2 && inputValue != 3 && inputValue != 4);

                    if (inputValue == 1)
                    {
                        //opcija 1 za testiranje
                        proxy.AddUser(Helper.ObjectToByteArray(new User("nemanja", "nemanja", "User")));

                        //opcija 2 za testiranje
                        //Console.WriteLine("Enter username: ");
                        //string username = Console.ReadLine();
                        //Console.WriteLine("Enter password:");
                        //password = Console.ReadLine();
                        //Console.WriteLine("Enter role:");
                        //string role = Console.ReadLine();
                        //proxy.AddUser(new User(username, password, role));

                    }

                    else if (inputValue == 2)
                    {
                        //ovo za testiranje         
                        User user = new User("marina", "marina", "User");
                        user.BetAccount.Amount = 65;
                        proxy.EditUser(Helper.ObjectToByteArray(user));

                        //ili ovo za testiranje
                        //Console.WriteLine("Enter username: ");
                        //string username = Console.ReadLine();
                        //User user2 = new User(username, username, "User");
                        //user2.BetAccount.Amount = 1000;
                        //proxy.EditUser(user2);
                    }
                    else if (inputValue == 3)
                    {
                        //ovo za testiranje         
                        proxy.DeleteUser(Helper.ObjectToByteArray("marina"));

                        //ili ovo za testiranje
                        //Console.WriteLine("Enter username: ");
                        //string username = Console.ReadLine();
                        //proxy.DeleteUser(username);
                    }
                    else if (inputValue == 4)
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
    }
}
