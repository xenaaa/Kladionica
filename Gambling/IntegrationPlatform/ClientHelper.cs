using Contracts;
using SecurityManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationPlatform
{
    public class ClientHelper : IClientHelperIntegration
    {
        //  IClientHelperIntegration proxy;
        ClientProxy proxy;//mora 2 proksija
        ClientPrintProxy printProxy;

        public ClientHelper() { }

        public bool CheckIfAlive(byte[] portBytes, byte[] addressBytes, byte[] isItPrintClientBytes)
        {
            Object obj = Helper.Decrypt(portBytes);
            int port = (int)obj;

            obj = Helper.Decrypt(addressBytes);
            string addressIPv4 = (string)obj;

            obj = Helper.Decrypt(isItPrintClientBytes);
            bool isItPrintClient = (bool)obj;

            NetTcpBinding binding = new NetTcpBinding();

            string address = "";


            if (isItPrintClient)
            {
                address = "net.tcp://" + addressIPv4 + ":" + port + "/ClientPrint";
                printProxy = new ClientPrintProxy(binding, address);
                return printProxy.CheckIfAlive();
            }
            else
            {
                address = "net.tcp://" + addressIPv4 + ":" + port + "/ClientHelper";
                proxy = new ClientProxy(binding, address);
                return proxy.CheckIfAlive();
            }

        }

        public bool CloseProxy()
        {
            return (proxy.CloseProxy() && printProxy.CloseProxy());
        }

        public bool GetServiceIP(byte[] AddressStringBytes)
        {
            string AddressString = Helper.Decrypt(AddressStringBytes) as string;

            Helper.BetServerAddress = AddressString;

            return true;
        }

        public bool SendGameResults(byte[] resultsBytes, byte[] portBytes, byte[] addressBytes)
        {
            Object obj = Helper.Decrypt(resultsBytes);
            byte[] results = Helper.ObjectToByteArray(obj);

            obj = Helper.Decrypt(portBytes);
            int port = (int)obj;

            obj = Helper.Decrypt(addressBytes);
            string addressIPv4 = (string)obj;

            NetTcpBinding binding = new NetTcpBinding();

            string address = "net.tcp://" + addressIPv4 + ":" + port + "/ClientPrint";

            printProxy = new ClientPrintProxy(binding, address);

            return printProxy.SendGameResults(results);
        }

        public bool SendOffers(byte[] offersBytes, byte[] portBytes, byte[] addressBytes, byte[] isItPrintClientBytes)
        {
            Object obj = Helper.Decrypt(offersBytes);
            byte[] offers = Helper.ObjectToByteArray(obj);

            obj = Helper.Decrypt(portBytes);
            int port = (int)obj;

            obj = Helper.Decrypt(addressBytes);
            string addressIPv4 = (string)obj;

            obj = Helper.Decrypt(isItPrintClientBytes);
            bool isItPrintClient = (bool)obj;

            NetTcpBinding binding = new NetTcpBinding();

            string address = "";
            if (isItPrintClient)
            {
                address = "net.tcp://" + addressIPv4 + ":" + port + "/ClientPrint";
                printProxy = new ClientPrintProxy(binding, address);
                return printProxy.SendOffers(offers);
            }
            else
            {
                address = "net.tcp://" + addressIPv4 + ":" + port + "/ClientHelper";
                proxy = new ClientProxy(binding, address);
                return proxy.SendOffers(offers);
            }
        }

        public bool SendTicketResults(byte[] ticketBytes, byte[] isPassedBytes, byte[] portBytes, byte[] addressBytes)
        {

            Object obj = Helper.Decrypt(ticketBytes);
            byte[] ticket = Helper.ObjectToByteArray(obj);

            obj = Helper.Decrypt(portBytes);
            int port = (int)obj;

            obj = Helper.Decrypt(addressBytes);
            string addressIPv4 = (string)obj;

            obj = Helper.Decrypt(isPassedBytes);
            byte[] isPassed = Helper.ObjectToByteArray(obj);

            NetTcpBinding binding = new NetTcpBinding();

            string address = "net.tcp://" + addressIPv4 + ":" + port + "/ClientHelper";

            proxy = new ClientProxy(binding, address);
            return proxy.SendTicketResults(ticket, isPassed);
        }
    }
}