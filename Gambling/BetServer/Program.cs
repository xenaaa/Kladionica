using BetServer;
using Contracts;
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

        private static object xmlLock = new object();

        public static object XMLLock
        {
            get { return xmlLock; }
            set { xmlLock = value; }
        }

        static void Main(string[] args)
        {
            NetTcpBinding binding = new NetTcpBinding();
            string address = "net.tcp://localhost:12208/BetService";

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

            //Thread.Sleep(15000);

            new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                SendGameResults(bs.Ports);
            }).Start();


            while (true)
            {
                CheckUserGames();
                Thread.Sleep(3000);
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
                lock (XMLLock)
                {
                    Thread.Sleep(15000);

                    if (File.Exists("offers.xml"))
                    {
                        xmlDoc.Load("offers.xml");

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


                            for (int i = 0; i < id.Count; i++)
                            {
                                Dictionary<int, double> odds = new Dictionary<int, double>();
                                odds.Add(1, Convert.ToDouble(kec[i].InnerText));
                                odds.Add(0, Convert.ToDouble(iks[i].InnerText));
                                odds.Add(2, Convert.ToDouble(dvojka[i].InnerText));

                                BetOffer bo = new BetOffer(home[i].InnerText, away[i].InnerText, Convert.ToInt32(id[i].InnerText), odds);
                                if (!Offers.ContainsKey(bo.Id))
                                    Offers.Add(bo.Id, bo);
                            }

                            lock (BetService.PortLock)
                            {
                                BetServerProxy proxy;
                                foreach (var port in ports)
                                {
                                    address = "net.tcp://localhost:9991/ClientIntegrationPlatform";
                                    proxy = new BetServerProxy(binding, address);
                                    {
                                        if (proxy.CheckIfAlive(port))
                                            proxy.SendOffers(Offers, port); //treba ports da mu proslijedi
                                    }

                                }
                                address = "net.tcp://localhost:" + 9995 + "/ClientPrint";
                                proxy = new BetServerProxy(binding, address);
                                {
                                    //if (proxy.CheckIfAlive(port))
                                    //    proxy.SendOffers(Offers, port);
                                    if (proxy.CheckIfAlive(9995))
                                        proxy.SendOffers(Offers, 9995);
                                }
                            }
                        }
                    }
                }
            }
        }

        private static bool CheckUserGames()//svake 2 sekunde proverava da li su se zavrsile sve utakmice na tiketima za svakog User-a pojedinacno. Ako su sve utakmice na tiketu zavrsene tikes se salje na proveru i brise
        {
            bool allGamesDone = true;
            Ticket t = new Ticket();

            List<Ticket> tickets = new List<Ticket>(); //lista tiketa koji se brisu iz liste

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
                                SendTicketResults2(user.Value, ticket);
                                tickets.Add(ticket);

                                //   //sendticketresoults za tog User-a i taj tiket...
                                //   new Thread(() =>
                                //   {
                                //       Thread.CurrentThread.IsBackground = true;
                                //       SendTicketResults2(user.Value, ticket);
                                //   }).Start();
                                ////   user.Value.Tickets.Remove(ticket);//uklanja se tiket

                                //   if (user.Value.Tickets.Count == 0)//ako obrise i poslednji tiket
                                //       break;
                            }
                        }

                        foreach (var item in tickets)
                        {
                            user.Value.Tickets.Remove(item);
                        }
                    }
                }
            }
            return true;
        }

        private static bool SendTicketResults2(User user, Ticket ticket)//sve utakmice na tiketu gotove, salje se ishod
        {
            bool won = true;

            //   List<string> results = new List<string>();

            foreach (KeyValuePair<int, Game> bet in ticket.Bets)
            {

                //   bet.Value.Result = BetService.Rezultati[bet.Key].Result;
                //  results.Add(BetService.Rezultati[bet.Key].Result);
                bet.Value.HomeGoalScored = BetService.Rezultati[bet.Key].HomeGoalScored;
                bet.Value.AwayGoalScored = BetService.Rezultati[bet.Key].AwayGoalScored;


                if (BetService.Rezultati[bet.Key].Tip != bet.Value.Tip)
                {
                    bet.Value.Won = false;
                    won = false;
                }
                else
                    bet.Value.Won = true;
            }

            NetTcpBinding binding = new NetTcpBinding();
            //    string address = "net.tcp://localhost:" + user.Port + "/ClientHelper";
            string address = "net.tcp://localhost:9991/ClientIntegrationPlatform";
            BetServerProxy proxy = new BetServerProxy(binding, address);
            {
                if (proxy.CheckIfAlive(user.Port))//ako vrati false obrisati tog user-a?
                    proxy.SendTicketResults(ticket, won, user.Port); // treba port od klijenta kom salje
            }

            if (won)
            {
                User changeUser = user;
                changeUser.BetAccount.Amount += ticket.CashPrize;
                BetService betService = new BetService();
                betService.EditUser(changeUser);
            }

            //user.Tickets.Clear();
            return true;
        }



        private static bool SendGameResults(List<int> ports)
        {
            List<Game> results = new List<Game>();
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
                Thread.Sleep(40000);

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
                    finished = r.Next(1, 6);  //broj utakmica koje ce se zavrsiti
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

                        tip = 0; //provjera ko je pobijedio
                        if (home > away)
                            tip = 1;
                        else if (home < away)
                            tip = 2;

                        Game game = new Game(betOffer, home, away, tip);

                        results.Add(game);

                        BetService.Rezultati.Add(betOffer.Id, game); //dodajemo utakmicu u listu zavrsenih utakmica                           

                        DeleteFinishedGame(betOffer.Id);

                        finished--;
                        j++;
                    } while (finished > 0);



                    //saljemo svima rezultate gotovih utakmica
                    lock (BetService.PortLock)
                    {
                        //foreach (var port in ports)//ne treba jer salje samo jednom zapravo
                        //{
                        //    //NetTcpBinding binding = new NetTcpBinding();
                        //    //string address = "net.tcp://localhost:" + port + "/ClientHelper";
                        //    //BetServerProxy proxy = new BetServerProxy(binding, address);
                        //    //{
                        //    //    if (proxy.CheckIfAlive())
                        //    //        proxy.SendGameResults(results);
                        //    //}
                        //    NetTcpBinding binding = new NetTcpBinding();
                        //  //  string address = "net.tcp://localhost:" + port + "/ClientHelper";
                        //   // string address = "net.tcp://localhost:9991/ClientIntegrationPlatform";
                        //    string address = "net.tcp://localhost:" + 9995 + "/ClientPrint";  //moramo dodati novi proxy

                        //    BetServerProxy proxy = new BetServerProxy(binding, address);
                        //    {
                        //        if (proxy.CheckIfAlive(port))
                        //            proxy.SendGameResults(results,port); //treba i port da se salje
                        //    }
                        //}

                        NetTcpBinding binding = new NetTcpBinding();
                        //  string address = "net.tcp://localhost:" + port + "/ClientHelper";
                        // string address = "net.tcp://localhost:9991/ClientIntegrationPlatform";
                        string address = "net.tcp://localhost:" + 9995 + "/ClientPrint";  //moramo dodati novi proxy

                        BetServerProxy proxy = new BetServerProxy(binding, address);
                        {
                            if (proxy.CheckIfAlive(9995))
                                proxy.SendGameResults(results, 9995); //treba i port da se salje
                        }
                    }
                }
            }
            return true;
        }

        private static void DeleteFinishedGame(int game)
        {
            XmlDocument xmlDoc = new XmlDocument(); // Create an XML document object
            lock (XMLLock)
            {
                if (File.Exists("offers.xml"))
                {
                    xmlDoc.Load("offers.xml");

                    Offers.Remove(game); //brisemo utakmicu iz liste ponuda
                    XmlNode node = xmlDoc.SelectSingleNode("descendant::PAIR[ID=" + game + "]");

                    if (node != null)
                    {
                        XmlNode parent = node.ParentNode;
                        parent.RemoveChild(node);
                    }

                    xmlDoc.Save("offers.xml");
                }
            }
        }
    }
}


