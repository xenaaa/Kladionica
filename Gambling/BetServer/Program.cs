using BetServer;
using IntegrationPlatform;
using System;
using System.Collections.Generic;
using System.IO;
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

            Thread.Sleep(15000);

            new Thread(() =>
        {
            Thread.CurrentThread.IsBackground = true;
            SendGameResults(bs.Ports);
        }).Start();


            while (true)
            {
                CheckUserGames();
                Thread.Sleep(2000);
            }

            host.Close();
        }

        private static void SendOffers(List<int> ports) //slanje ponude kijentu svakih 5 minuta
        {
            NetTcpBinding binding = new NetTcpBinding();
            string address = "";

            XmlDocument xmlDoc = new XmlDocument(); // Create an XML document object

            while (true)
            {
                if (File.Exists("offer.xml"))
                {
                    xmlDoc.Load("offer.xml");

                    XmlNode node = xmlDoc.SelectSingleNode("descendant::OFFER");

                    if (node.ChildNodes.Count > 0)
                    {
                        // Get elements
                        XmlNodeList id = xmlDoc.GetElementsByTagName("ID");
                        XmlNodeList home = xmlDoc.GetElementsByTagName("HOME");
                        XmlNodeList away = xmlDoc.GetElementsByTagName("AWAY");
                        XmlNodeList kec = xmlDoc.GetElementsByTagName("ONE");
                        XmlNodeList iks = xmlDoc.GetElementsByTagName("X");
                        XmlNodeList dvojka = xmlDoc.GetElementsByTagName("TWO");

                        Dictionary<int, double> odds = new Dictionary<int, double>();
                        odds.Add(1, Convert.ToDouble(kec[0].InnerText));
                        odds.Add(0, Convert.ToDouble(iks[0].InnerText));
                        odds.Add(2, Convert.ToDouble(dvojka[0].InnerText));

                        for (int i = 0; i < id.Count; i++)
                        {
                            BetOffer bo = new BetOffer(home[i].InnerText, away[i].InnerText, Convert.ToInt32(id[i].InnerText), odds);
                            if (!Offers.ContainsKey(bo.Id))
                                Offers.Add(bo.Id, bo);
                        }

                        lock (BetService.PortLock)
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
                        }
                    }

                }
                Thread.Sleep(5000);
            }
        }

        private static bool CheckUserGames()//svake 2 sekunde proverava da li su se zavrsile sve utakmice na tiketima za svakog User-a pojedinacno. Ako su sve utakmice na tiketu zavrsene tikes se salje na proveru i brise
        {
            bool allGamesDone = true;
            Ticket t = new Ticket();


            if (BetService.BetUsers.Count > 0 && BetService.Rezultati.Count > 0)
            {
                foreach (KeyValuePair<string, User> user in BetService.BetUsers)
                {
                    if (user.Value.Tickets.Count > 0)
                    {
                        foreach (Ticket ticket in user.Value.Tickets)
                        {
                            if (ticket.Bets.Count > 0)
                            {
                                foreach (KeyValuePair<int, Game> bet in ticket.Bets)
                                {
                                    if (BetService.Rezultati.ContainsKey(bet.Key))
                                    {
                                        continue;//utakmica zavrsena
                                    }
                                    else
                                    {
                                        //utakmica nije gotova
                                        allGamesDone = false;
                                        break;//prelazi se na sledeci tiket istog User-a
                                    }
                                }
                            }
                            else
                                continue;

                            if (allGamesDone)
                            {
                                //sendticketresoults za tog User-a i taj tiket...
                                new Thread(() =>
                                {
                                    Thread.CurrentThread.IsBackground = true;
                                    SendTicketResults2(user.Value, ticket);
                                }).Start();
                                user.Value.Tickets.Remove(ticket);//uklanja se tiket

                                if (user.Value.Tickets.Count == 0)//ako obrise i poslednji tiket
                                    break;
                            }
                        }
                    }
                }
            }
            return true;
        }

        private static bool SendTicketResults2(User user, Ticket ticket)//sve utakmice na tiketu gotove, salje se ishod
        {
            bool won = true;

            List<string> results = new List<string>();

            foreach (KeyValuePair<int, Game> bet in ticket.Bets)
            {
           
                bet.Value.Result = BetService.Rezultati[bet.Key].Result;
                results.Add(BetService.Rezultati[bet.Key].Result);

                if (BetService.Rezultati[bet.Key].Tip != bet.Value.Tip)
                {
                    bet.Value.Won = false;
                    won = false;
                }
                else
                    bet.Value.Won = true;
            }

            NetTcpBinding binding = new NetTcpBinding();
            string address = "net.tcp://localhost:" + user.Port + "/ClientHelper";
            BetServerProxy proxy = new BetServerProxy(binding, address);
            {
                if (proxy.CheckIfAlive())//ako vrati false obrisati tog user-a?
                    proxy.SendTicketResults(ticket, won, results);
            }
            //user.Tickets.Clear();
            return true;
        }



        private static bool SendGameResults(List<int> ports)
        {
            List<string> results = new List<string>();
            List<int> gameIDs = new List<int>();
            BetOffer betOffer = new BetOffer();
            List<int> indexToDelete = new List<int>();

            int index;
            int home;
            int away;
            int tip;
            int offersNumber;
            int finished;
            int j;

            while (true)
            {
                j = 0;
                if (Offers.Count > 0)
                {
                    offersNumber = Offers.Count();

                    if (gameIDs.Count > 0)
                        gameIDs.Clear();

                    if (results.Count > 0)
                        results.Clear();

                    if (indexToDelete.Count > 0)
                        indexToDelete.Clear();

                    foreach (var offer in Offers)
                    {
                        gameIDs.Add(offer.Key);  //dodajemo u listu sifre svih utakmica da bi mogli nasumicno izabrati nekoliko
                    }

                    Random r = new Random();
                    finished = r.Next(1, 5);  //broj utakmica koje ce se zavrsiti
                    if (finished > Offers.Count)
                        finished = Offers.Count;

                    for (int i = 0; i < finished; i++)
                    {
                        do
                        {
                            index = r.Next(0, offersNumber);
                            if (!indexToDelete.Contains(index))
                            {
                                indexToDelete.Add(index);
                                break;
                            }
                        } while (indexToDelete.Contains(index));
                    }

                    do
                    {
                        betOffer = Offers[gameIDs[indexToDelete[j]]]; //izvlacimo tu utakmicu
                        home = r.Next(0, 5); //broj datih golova
                        away = r.Next(0, 5);

                        string res = betOffer.Id.ToString() + "     " + betOffer.Home.ToString() + "  :  " + betOffer.Away.ToString() + "  -  " + home + "  :  " + away;
                        results.Add(res);

                        tip = 0; //provjera ko je pobijedio
                        if (home > away)
                            tip = 1;
                        else if (home < away)
                            tip = 2;
   
                        string result = home.ToString() + ":" + away.ToString();
                        Game game = new Game(betOffer, result, tip);
                     
                        BetService.Rezultati.Add(betOffer.Id, game); //dodajemo utakmicu u listu zavrsenih utakmica                           

                        DeleteFinishedGame(betOffer.Id);

                        finished--;
                        j++;
                    } while (finished > 0);



                    //saljemo svima rezultate gotovih utakmica
                    lock (BetService.PortLock)
                    {
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
                    }
                    Thread.Sleep(15000);
                }
            }
            return true;
        }

        private static void DeleteFinishedGame(int game)
        {
            XmlDocument xmlDoc = new XmlDocument(); // Create an XML document object

            if (File.Exists("offer.xml"))
            {
                xmlDoc.Load("offer.xml");

                Offers.Remove(game); //brisemo utakmicu iz liste ponuda
                XmlNode node = xmlDoc.SelectSingleNode("descendant::PAIR[ID=" + game + "]");

                if (node != null)
                {
                    XmlNode parent = node.ParentNode;
                    parent.RemoveChild(node);
                }

                xmlDoc.Save("offer.xml");
            }
        }
    }
}


