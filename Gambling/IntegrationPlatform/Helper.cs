using SecurityManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;

namespace Contracts
{
    public static class Helper
    {
        public const string key = "123456781234567812345678";
        public const int clientPrintPort = 22222;
        public const int betServicePort = 12208;
        public const int bankServicePort = 15000;
        public const int integrationHostPort = 9991;

        public static byte[] ObjectToByteArray(object obj)
        {
            using (System.IO.MemoryStream stream = new System.IO.MemoryStream())
            {
                System.Runtime.Serialization.Formatters.Binary.BinaryFormatter formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                formatter.Serialize(stream, obj);

                byte[] bytes = stream.ToArray();
                stream.Flush();

                return bytes;
            }
        }


        public static Object ByteArrayToObject(byte[] arrBytes)
        {
           using (System.IO.MemoryStream stream = new System.IO.MemoryStream(arrBytes))
            {
                stream.Position = 0;
                object desObj = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter().Deserialize(stream);
                return desObj;
           }

        }

        public static Object Decrypt(byte[] bytes)
        {
            RivestChest4Algorithm rc4 = new RivestChest4Algorithm();

            byte[] decrypted = rc4.Decrypt(key, bytes);
            object obj = Helper.ByteArrayToObject(decrypted);
            return obj;
        }

        public static byte[] Encrypt(Object obj)
        {
            RivestChest4Algorithm rc4 = new RivestChest4Algorithm();
            byte[] objectByte = Helper.ObjectToByteArray(obj);
            byte[] encrypted = rc4.Encrypt(key, objectByte);
            return encrypted;
        }

        public static byte[] EncryptOnIntegration(byte[] obj)
        {
            RivestChest4Algorithm rc4 = new RivestChest4Algorithm();
            byte[] encrypted = rc4.Encrypt(key, obj);
            return encrypted;
        }

        public static string GetIP()
        {
            string addressIPv4 = string.Empty;

            OperationContext oOperationContext = OperationContext.Current;
            MessageProperties oMessageProperties = oOperationContext.IncomingMessageProperties;

            RemoteEndpointMessageProperty endpoint = oMessageProperties[RemoteEndpointMessageProperty.Name] as RemoteEndpointMessageProperty;

            string addressIPv6 = endpoint.Address;
            int nPort = endpoint.Port;

            IPAddress ipAddress = IPAddress.Parse(addressIPv6);
            if (ipAddress.AddressFamily == AddressFamily.InterNetwork)
                return addressIPv6;

                addressIPv4 = addressIPv6;//ako je vracena adresa vec zapravo IPv4, moze se dasiti...


            //byte[] encryptedAddress = Helper.Encrypt(addressIPv6);

            
           
            IPHostEntry ipHostEntry = Dns.GetHostEntry(ipAddress);
            foreach (IPAddress address in ipHostEntry.AddressList)
            {
                if (address.AddressFamily == AddressFamily.InterNetwork)
                    addressIPv4 = address.ToString();

            }

            return addressIPv4;
        }
    }
}
