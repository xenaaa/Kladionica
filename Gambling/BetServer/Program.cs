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
        private static Dictionary<int, BetOffer> Offers = new Dictionary<int, BetOffer>();
        static void Main(string[] args)
        {
            NetTcpBinding binding = new NetTcpBinding();
            string address = "net.tcp://localhost:9998/BetService";

            ServiceHost host = new ServiceHost(typeof(BetService));
            host.AddServiceEndpoint(typeof(IBetService), binding, address);

            host.Open();

            Console.WriteLine("Bet service is started.");
            Console.WriteLine("Press <enter> to stop service...");

            BetService bs = new BetService();

            new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                SendOffers(bs.Ports);
            }).Start();
            Thread.Sleep(10000);

            //  new Thread(() =>
            //  {
            //  Thread.CurrentThread.IsBackground = true;
            SendGameResults(bs.Ports);
            //   }).Start();

            Thread.Sleep(5000);

            while (true)
            {
                SendTicketResults();
                Thread.Sleep(2000);
            }

            Console.ReadLine();
            host.Close();
        }

        private static void SendOffers(List<int> ports) //slanje ponude kijentu svakih 5 minuta
        {
            NetTcpBinding binding = new NetTcpBinding();
            string address = "";

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
                    Offers.Add(bo.Id, bo);
                }

            while (true)
            {
                foreach (var port in ports)
                {
                    address = "net.tcp://localhost:" + port + "/ClientHelper";
                    BetServerProxy proxy = new BetServerProxy(binding, address);
                    {
                        if (proxy.CheckIfAlive())
                            proxy.SendOffers(Offers);
                    }
                }
                Thread.Sleep(3000);
            }
        }

        private static bool SendTicketResults()
        {
            bool won = true;
            Ticket t = new Ticket();

            if (BetService.BetUsers.Count > 0 && BetService.Rezultati.Count > 0)
            {
                foreach (KeyValuePair<string, User> user in BetService.BetUsers)
                {
                    foreach (Ticket tiket in user.Value.Tickets)
                    {
                        if (tiket.Bets.Count > 0)
                            foreach (KeyValuePair<int, Game> bet in tiket.Bets)
                            {
                                if (BetService.Rezultati.ContainsKey(bet.Key))//ne sme biti prazan tiket
                                {
                                    if (!BetService.Rezultati[bet.Key].ContainsKey(bet.Value.Tip))
                                    {
                                        bet.Value.Won = false;
                                        won = false;
                                    }
                                    else
                                        bet.Value.Won = true;
                                }
                            }
                        else
                            continue;
                        t = tiket;
                    }

                    NetTcpBinding binding = new NetTcpBinding();
                    string address = "net.tcp://localhost:" + user.Value.Port + "/ClientHelper";
                    BetServerProxy proxy = new BetServerProxy(binding, address);
                    {
                        if (proxy.CheckIfAlive())
                            proxy.SendTicketResults(t, won);
                    }

                    user.Value.Tickets.Clear();
                }
            }
            return true;
        }

        private static bool SendGameResults(List<int> ports)
        {
            List<string> results = new List<string>();


            //   while (true)
            {
                int offersNumber = Offers.Count;
                int[] niz = new int[] { 1001, 2002, 3002 };

                Random r = new Random();
                int finished = 3;
                int i = 0;
                do
                {
                    int index = r.Next(0, offersNumber);
                    BetOffer bo = Offers[niz[i]];
                    int home = r.Next(0, 5);
                    int away = r.Next(0, 5);
                    string res = bo.Home.ToString() + "  :  " + bo.Away.ToString() + "  -  " + home + "  :  " + away;
                    results.Add(res);

                    int tip = 0;
                    if (home > away)
                        tip = 1;
                    else if (home < away)
                        tip = 2;

                    Dictionary<int, double> dictionary2 = new Dictionary<int, double>();
                    dictionary2.Add(tip, Offers[bo.Id].Odds[tip]);

                    BetService.Rezultati.Add(bo.Id, dictionary2);

                    finished--;
                    i++;
                } while (finished > 0);

                foreach (var port in ports)
                {
                    NetTcpBinding binding = new NetTcpBinding();
                    string address = "net.tcp://localhost:" + port + "/ClientHelper";
                    BetServerProxy proxy = new BetServerProxy(binding, address);
                    {
                        if (proxy.CheckIfAlive())
                            proxy.SendGameResults(results);
                    }
                }

                Thread.Sleep(5000);
            }
            return true;
        }
    }
}