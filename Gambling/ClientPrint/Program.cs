using Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace ClientPrint
{
    class Program
    {
        static void Main(string[] args)
        {
            string address = "net.tcp://localhost:" + args[0] + "/ClientPrint";

            Console.WriteLine(args[0]);

            NetTcpBinding binding = new NetTcpBinding();
            ServiceHost host = new ServiceHost(typeof(ClientPrint));
            host.AddServiceEndpoint(typeof(IClientHelper), binding, address);
            try
            {
                host.Open();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            Console.ReadLine();
        }
    }
}