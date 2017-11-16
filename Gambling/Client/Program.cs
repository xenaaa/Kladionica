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

            bool error = false;
            int input = 0;
            NetTcpBinding binding = new NetTcpBinding();
            string address = "";

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
                        address = "net.tcp://localhost:9999/BankService";

                        using (ClientBankProxy proxy = new ClientBankProxy(binding, address))
                        {

                        }
                        break;
                    }

                case 2:
                    {
                        address = "net.tcp://localhost:9999/BetService";

                        using (ClientBetProxy proxy = new ClientBetProxy(binding, address))
                        {
                            User user = new User("marina", "la", "Admin");
                            proxy.AddUser(user);
                            proxy.AddUser(user);
                        }
                        break;
                    }
            }

            Console.ReadLine();
        }
    }
}
