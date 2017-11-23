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
    public class ClientHelper : IClientHelper
    {
        ClientProxy proxy;

        public ClientHelper() { }

        public bool CheckIfAlive(byte[] portBytes, byte[] addressBytes)
        {
            Object obj = Helper.Decrypt(portBytes);
            int port = (int)obj;

            obj = Helper.Decrypt(addressBytes);
            string addressIPv4 = (string)obj;

            NetTcpBinding binding = new NetTcpBinding();

            string address = "";


            if (port == Helper.clientPrintPort)
                address = "net.tcp://"+ addressIPv4 + ":" + port + "/ClientPrint";
            else
                address = "net.tcp://"+ addressIPv4 + ":" + port + "/ClientHelper";

            proxy = new ClientProxy(binding, address);
            return proxy.CheckIfAlive(portBytes, addressBytes);
        }

        public bool GetServiceIP(byte[] AddressStringBytes)//proveriti da li se ovo desilo
        {
            string AddressString = Helper.Decrypt(AddressStringBytes) as string;


            if (AddressString.Contains("BankService"))//banka salje adresu
            {
                Helper.BankServerAddress = AddressString;
            }
            else//bet salje adresu
            {
                Helper.BetServerAddress = AddressString;
            }

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

            string address = "net.tcp://"+ addressIPv4 + ":" + port + "/ClientPrint";

            proxy = new ClientProxy(binding, address);
            return proxy.SendGameResults(results, portBytes, addressBytes);
        }

        public bool SendOffers(byte[] offersBytes, byte[] portBytes, byte[] addressBytes)
        {
            Object obj = Helper.Decrypt(offersBytes);
            byte[] offers = Helper.ObjectToByteArray(obj);

            obj = Helper.Decrypt(portBytes);
            byte[] portB = Helper.ObjectToByteArray(obj);
            int port = (int)obj;

            obj = Helper.Decrypt(addressBytes);
            byte[] addressb = Helper.ObjectToByteArray(obj);
            string addressIPv4 = (string)obj;

            NetTcpBinding binding = new NetTcpBinding();

            string address = "";
            if (port == Helper.clientPrintPort)
                address = "net.tcp://"+ addressIPv4 + ":" + port + "/ClientPrint";
            else
                address = "net.tcp://"+ addressIPv4 + ":" + port + "/ClientHelper";

            proxy = new ClientProxy(binding, address);
            return proxy.SendOffers(offers, portB, addressb);
        }

        public bool SendTicketResults(byte[] ticketBytes, byte[] isPassedBytes, byte[] portBytes,byte[] addressBytes)
        {

            Object obj = Helper.Decrypt(ticketBytes);
            byte[] ticket = Helper.ObjectToByteArray(obj);

            obj = Helper.Decrypt(portBytes);
            byte[] portB = Helper.ObjectToByteArray(obj);
            int port = (int)obj;

            obj = Helper.Decrypt(addressBytes);
            byte[] addressB = Helper.ObjectToByteArray(obj);
            string addressIPv4 = (string)obj;

            obj = Helper.Decrypt(isPassedBytes);
            byte[] isPassed = Helper.ObjectToByteArray(obj);

            NetTcpBinding binding = new NetTcpBinding();

            string address = "net.tcp://"+ addressIPv4 + ":" + port + "/ClientHelper";

            proxy = new ClientProxy(binding, address);
            return proxy.SendTicketResults(ticket, isPassed, portB, addressB);
        }


    }
}