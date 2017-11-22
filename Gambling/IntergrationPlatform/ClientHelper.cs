using Contracts;
using SecurityManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace IntergrationPlatform
{
    public class ClientHelper : IClientHelper
    {
        ClientProxy proxy;

        public ClientHelper() { }

        public bool CheckIfAlive(byte[] portBytes)
        {
            Object obj = Helper.Decrypt(portBytes);
            int port = (int)obj;

            NetTcpBinding binding = new NetTcpBinding();

            string address = "";


            if (port == Helper.clientPrintPort)
                address = "net.tcp://localhost:" + port + "/ClientPrint";
            else
                address = "net.tcp://localhost:" + port + "/ClientHelper";

            proxy = new ClientProxy(binding, address);
            return proxy.CheckIfAlive(portBytes);
        }

        public bool SendGameResults(byte[] resultsBytes, byte[] portBytes)
        {
            Object obj = Helper.Decrypt(resultsBytes);
            byte[] results = Helper.ObjectToByteArray(obj);

            obj = Helper.Decrypt(portBytes);
            int port = (int)obj;

            NetTcpBinding binding = new NetTcpBinding();

            string address = "net.tcp://localhost:" + port + "/ClientPrint";

            proxy = new ClientProxy(binding, address);
            return proxy.SendGameResults(results, portBytes);
        }

        public bool SendOffers(byte[] offersBytes, byte[] portBytes)
        {
            Object obj = Helper.Decrypt(offersBytes);
            byte[] offers = Helper.ObjectToByteArray(obj);

            obj = Helper.Decrypt(portBytes);
            byte[] portB = Helper.ObjectToByteArray(obj);
            int port = (int)obj;

            NetTcpBinding binding = new NetTcpBinding();

            string address = "";
            if (port == Helper.clientPrintPort)
                address = "net.tcp://localhost:" + port + "/ClientPrint";
            else
                address = "net.tcp://localhost:" + port + "/ClientHelper";

            proxy = new ClientProxy(binding, address);
            return proxy.SendOffers(offers, portB);
        }

        public bool SendTicketResults(byte[] ticketBytes, byte[] isPassedBytes, byte[] portBytes)
        {

            Object obj = Helper.Decrypt(ticketBytes);
            byte[] ticket = Helper.ObjectToByteArray(obj);

            obj = Helper.Decrypt(portBytes);
            byte[] portB = Helper.ObjectToByteArray(obj);
            int port = (int)obj;

            obj = Helper.Decrypt(isPassedBytes);
            byte[] isPassed = Helper.ObjectToByteArray(obj);

            NetTcpBinding binding = new NetTcpBinding();

            string address = "net.tcp://localhost:" + port + "/ClientHelper";

            proxy = new ClientProxy(binding, address);
            return proxy.SendTicketResults(ticket, isPassed, portB);
        }


    }
}