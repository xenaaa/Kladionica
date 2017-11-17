using IntegrationPlatform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            NetTcpBinding binding = new NetTcpBinding();
            int port = Convert.ToInt32(args[0]);
            string address = "net.tcp://localhost:"+port+"/ClientHelper";

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


            switch (input)
            {
                case 1:
                    {
                        Console.WriteLine("Enter username:");
                        string username = Console.ReadLine();
                        Console.WriteLine("Enter password:");
                        string password = Console.ReadLine();

                        //    User user = new User(username, password, "User");

                        address = "net.tcp://localhost:9999/BankService";

                        ClientBankProxy proxy = new ClientBankProxy(binding, address);

                        if (proxy.CheckIfAlive())
                        {
                            proxy.Login(username, password);
                            //User user = new User("marina", "la", "Admin");
                            //User user2 = new User("david", "la", "Admin");
                            //proxy.CreateAccount(user);
                            //proxy.CreateAccount(user2);
                            Account depAcc = new Account(3, 11);
                            proxy.Deposit(depAcc, username);
                        }
                        else
                            Console.WriteLine("Server is down");
                        break;
                    }

                case 2:
                    {
                        address = "net.tcp://localhost:9999/BetService";

                        ClientBetProxy proxy = new ClientBetProxy(binding, address);

                        if (proxy.CheckIfAlive())
                        {
                            proxy.SendPort(port);

                            User user = new User("marina", "la", "Admin");
                            proxy.AddUser(user);
                            proxy.AddUser(user);
                        }
                        else
                            Console.WriteLine("Server is down");
                        break;
                    }
            }

            Console.ReadLine();
            host.Close();
        }
    }
}
