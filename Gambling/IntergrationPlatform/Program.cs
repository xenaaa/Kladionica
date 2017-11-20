using Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace IntergrationPlatform
{
    class Program
    {
        static void Main(string[] args)
        {
            NetTcpBinding binding = new NetTcpBinding();

            string address = "net.tcp://localhost:9991/BetIntegrationPlatform";
            ServiceHost host = new ServiceHost(typeof(BetService));
            host.AddServiceEndpoint(typeof(IBetService), binding, address);
            host.Open();
            Console.WriteLine("Bet Integration Platform service is started.");
            Console.WriteLine("Press <enter> to stop service...");


            address = "net.tcp://localhost:9991/BankIntegrationPlatform";
            ServiceHost host2 = new ServiceHost(typeof(BankService));
            host2.AddServiceEndpoint(typeof(IBankService), binding, address);
            host2.Open();
            Console.WriteLine("Bank Integration Platform is started.");
            Console.WriteLine("Press <enter> to stop service...");



            address = "net.tcp://localhost:9991/ClientIntegrationPlatform";
            ServiceHost host3 = new ServiceHost(typeof(ClientHelper));
            host3.AddServiceEndpoint(typeof(IClientHelper), binding, address);
            host3.Open();
            Console.WriteLine("Client Integration Platform is started.");
            Console.WriteLine("Press <enter> to stop service...");


            Console.ReadLine();
            host.Close();
            host2.Close();
            host3.Close();
        }
    }
}
