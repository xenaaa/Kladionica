using IntegrationPlatform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            NetTcpBinding binding = new NetTcpBinding();
            int port = Convert.ToInt32(Console.ReadLine());
            //int port = Convert.ToInt32(args[0]);
            string address = "net.tcp://localhost:" + port + "/ClientHelper";

            ServiceHost host = new ServiceHost(typeof(ClientHelper));
            host.AddServiceEndpoint(typeof(IClientHelper), binding, address);

            host.Open();

            Console.WriteLine("Client service is started.");
            Console.WriteLine("Press <enter> to stop service...");

            bool error = false;
            int input = 0;
            //   NetTcpBinding binding = new NetTcpBinding();
            //   string address = "";

            do
            {
                Console.WriteLine("     MENU\n");
                Console.WriteLine(" Press 1 - Bank service");
                Console.WriteLine(" Press 2 - Bet service");

                try
                {
                    input = Convert.ToInt32(Console.ReadLine());
                    error = true;
                }
                catch
                {
                    Console.WriteLine("Wrong input. Try again.");
                    error = false;
                }
            } while (!error);



            NetTcpBinding binding2 = new NetTcpBinding();
            binding2.Security.Mode = SecurityMode.Transport;
            binding2.Security.Transport.ProtectionLevel = System.Net.Security.ProtectionLevel.EncryptAndSign;
            binding2.Security.Transport.ClientCredentialType = TcpClientCredentialType.Windows;

            WindowsIdentity clientIdentity = WindowsIdentity.GetCurrent();

            foreach (IdentityReference group in clientIdentity.Groups)
            {
                SecurityIdentifier sid = (SecurityIdentifier)group.Translate(typeof(SecurityIdentifier));
                var name = sid.Translate(typeof(NTAccount));
                if (name.ToString().Contains("\\BetUser") || name.ToString().Contains("\\BetReader") || name.ToString().Contains("\\BBAdmin"))
                    Console.WriteLine(name.ToString());
            }

            switch (input)
            {
                case 1:
                    {
                        Console.WriteLine("Your username is: " + clientIdentity.Name);
                        Console.WriteLine("Enter password:");
                        string password = Console.ReadLine();

                        //    User user = new User(username, password, "User");

                        address = "net.tcp://localhost:9999/BankService";

                        ClientBankProxy proxy = new ClientBankProxy(binding, address);

                        if (proxy.CheckIfAlive())
                        {
                            proxy.Login(clientIdentity.Name, password,port);
                            //User user = new User("marina", "la", "Admin");
                            //User user2 = new User("david", "la", "Admin");
                            //proxy.CreateAccount(user);
                            //proxy.CreateAccount(user2);
                            Account depAcc = new Account(3, 11);
                            proxy.Deposit(depAcc);

                            
                        }
                        else
                            Console.WriteLine("Server is down");
                        break;
                    }

                case 2:
                    {
                        Console.WriteLine("Your username is: " + clientIdentity.Name);
                        Console.WriteLine("Enter password:");
                        string password = Console.ReadLine();

                        address = "net.tcp://localhost:9998/BetService";

                        ClientBetProxy proxy = new ClientBetProxy(binding, address);

                        if (proxy.CheckIfAlive())
                        {
                            proxy.SendPort(port);

                            if (proxy.Login(clientIdentity.Name, password,port))
                            {
                                Thread.Sleep(4000);
                                MakeTicket(proxy);
                            }
                        }
                        else
                            Console.WriteLine("Server is down");
                        break;
                    }
            }

            Console.ReadLine();
            host.Close();
        }


        private static void MakeTicket(ClientBetProxy proxy)
        {
            WindowsIdentity clientIdentity = WindowsIdentity.GetCurrent();
            Ticket t = new Ticket();
            t.Payment = 5;
            Dictionary<int, Game> bets = new Dictionary<int, Game>();

            Game g = new Game();
            g.Odds = ClientHelper.Offers[1001].Odds[1];
            g.Tip = 1;
            g.Won = false;
            bets.Add(1001, g);
            g.Odds = ClientHelper.Offers[2002].Odds[0];
            g.Tip = 0;
            g.Won = false;
            bets.Add(2002, g);
            g.Odds = ClientHelper.Offers[3002].Odds[2];
            g.Tip = 2;
            g.Won = false;
            bets.Add(3002, g);

            t.Bets = bets;

            proxy.SendTicket(t, clientIdentity.Name);
        }
    }
}
