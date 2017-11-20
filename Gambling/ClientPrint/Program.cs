﻿using IntegrationPlatform;
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
            
            string address = "net.tcp://localhost:" + 10011 + "/ClientPrint";

            NetTcpBinding binding = new NetTcpBinding();
            ServiceHost host = new ServiceHost(typeof(ClientPrint));
            host.AddServiceEndpoint(typeof(IClientHelper), binding, address);

            host.Open();

            Console.ReadLine();
        }
    }
}
