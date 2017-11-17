using BetServer;
using IntegrationPlatform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace BetServer
{
    class Program
    {
        static void Main(string[] args)
        {
            NetTcpBinding binding = new NetTcpBinding();
            string address = "net.tcp://localhost:9999/BetService";

            ServiceHost host = new ServiceHost(typeof(BetService));
            host.AddServiceEndpoint(typeof(IBetService), binding, address);

            host.Open();

            Console.WriteLine("Bet service is started.");
            Console.WriteLine("Press <enter> to stop service...");

            List<BetOffer> offers = new List<BetOffer>();

            XmlDocument xmlDoc = new XmlDocument(); // Create an XML document object
            xmlDoc.Load("lista.xml"); // Load the XML document from the specified file

            // Get elements
            XmlNodeList id = xmlDoc.GetElementsByTagName("ID");
            XmlNodeList home = xmlDoc.GetElementsByTagName("DOMACIN");
            XmlNodeList away = xmlDoc.GetElementsByTagName("GOST");
            XmlNodeList kec = xmlDoc.GetElementsByTagName("KEC");
            XmlNodeList iks = xmlDoc.GetElementsByTagName("IKS");
            XmlNodeList dvojka = xmlDoc.GetElementsByTagName("DVOJKA");

            Dictionary<int, double> odds = new Dictionary<int, double>();
            odds.Add(1, Convert.ToDouble(kec[0].InnerText));
            odds.Add(0, Convert.ToDouble(iks[0].InnerText));
            odds.Add(2, Convert.ToDouble(dvojka[0].InnerText));

            for (int i = 0; i < id.Count; i++)
            {
                BetOffer bo = new BetOffer(home[i].InnerText, away[i].InnerText, Convert.ToInt32(id[i].InnerText), odds);
                offers.Add(bo);
            }

         //   Thread.Sleep(4000);

            address = "net.tcp://localhost:9997/ClientHelper";

           /*while (true)
            {
                try
                {
                    using (BetServerProxy proxy = new BetServerProxy(binding, address))
                    {
                        proxy.SendOffers(offers);
                    }
                    Thread.Sleep(4000);
                }
                catch { }
            }*/

            Console.ReadLine();
            host.Close();
        }
    }
}
