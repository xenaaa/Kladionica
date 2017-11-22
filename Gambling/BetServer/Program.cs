using BetServer;
using CertificateManager;
using Contracts;
using SecurityManager;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.ServiceModel.Security;
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
        private static object resultsLock = new object();

        private static bool sendOffers = false;
        private static bool checkUserGames = false;

        public static object XMLLock
        {
            get { return xmlLock; }
            set { xmlLock = value; }
        }

        public static object ResultsLock
        {
            get { return resultsLock; }
            set { resultsLock = value; }
        }

        static void Main(string[] args)
        {
            string srvCertCN = "betservice";

            NetTcpBinding binding = new NetTcpBinding();
            binding.Security.Transport.ClientCredentialType = TcpClientCredentialType.Certificate;
            string address = "net.tcp://localhost:" + Helper.betServicePort + "/BetService";

            ServiceHost host = new ServiceHost(typeof(BetService));
            host.AddServiceEndpoint(typeof(IBetService), binding, address);


            //sertifikacija
            host.Credentials.ClientCertificate.Authentication.CertificateValidationMode = X509CertificateValidationMode.ChainTrust;
            host.Credentials.ClientCertificate.Authentication.RevocationMode = X509RevocationMode.NoCheck;

            ///Set appropriate service's certificate on the host. Use CertManager class to obtain the certificate based on the "srvCertCN"
            host.Credentials.ServiceCertificate.Certificate = CertManager.GetCertificateFromStorage(StoreName.My, StoreLocation.LocalMachine, srvCertCN);


            host.Open();

            Console.WriteLine("Bet service is started.");
            Console.WriteLine("Press <enter> to stop service...");

            BetService bs = new BetService();

            new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                SendOffers();
            }).Start();

            //Thread.Sleep(15000);

            new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                SendGameResults();
            }).Start();


            while (true)
            {
                if (checkUserGames)
                    CheckUserGames();
                //    DateTime start = DateTime.Now;
                //    do
                //    {
                //        if (checkUserGames)
                //        {
                //            break;
                //        }            
                //    } while (start.AddSeconds(3) > DateTime.Now);
            }

            host.Close();
        }

        private static void SendOffers() //slanje ponude kijentu svakih 5 minuta
        {
            NetTcpBinding binding = new NetTcpBinding();

            XmlDocument xmlDoc = new XmlDocument(); // Create an XML document object

            while (true)
            {
                lock (XMLLock)
                {
                    // Thread.Sleep(15000);

                    DateTime start = DateTime.Now;
                    //   DateTime now;

                    do
                    {
                        if (sendOffers)
                        {
                            break;
                        }
                        //     now = DateTime.Now;
                    } while (start.AddSeconds(15) > DateTime.Now);

                    sendOffers = false;

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
                                string srvCertCN = "betserviceintegration";
                                binding = new NetTcpBinding();
                                binding.Security.Transport.ClientCredentialType = TcpClientCredentialType.Certificate;

                                X509Certificate2 srvCert = CertManager.GetCertificateFromStorage(StoreName.My, StoreLocation.LocalMachine, srvCertCN);
                                EndpointAddress address = new EndpointAddress(new Uri("net.tcp://localhost:" + Helper.integrationHostPort + "/ClientIntegrationPlatform"),
                                                          new X509CertificateEndpointIdentity(srvCert));

                                BetServerProxy proxy;
                                proxy = new BetServerProxy(binding, address);

                                byte[] encryptedPort;


                                List<int> portsFromFile = new List<int>();
                                Object obj = Persistance.ReadFromFile("ports");
                                if (obj != null)
                                    portsFromFile = (List<int>)obj;

                                foreach (var port in portsFromFile)
                                {
                                    encryptedPort = Helper.Encrypt(port);

                                    if (proxy.CheckIfAlive(encryptedPort))
                                    {
                                        byte[] encryptedOffers = Helper.Encrypt(Offers);
                                        proxy.SendOffers(encryptedOffers, encryptedPort);
                                    }
                                }

                                encryptedPort = Helper.Encrypt(Helper.clientPrintPort);

                                if (proxy.CheckIfAlive(encryptedPort))
                                {
                                    byte[] encryptedOffers = Helper.Encrypt(Offers);
                                    proxy.SendOffers(encryptedOffers, encryptedPort);
                                }
                            }
                        }
                    }
                }
            }
        }

        private static bool CheckUserGames()//svake 2 sekunde proverava da li su se zavrsile sve utakmice na tiketima za svakog User-a pojedinacno. Ako su sve utakmice na tiketu zavrsene tikes se salje na proveru i brise
        {
            checkUserGames = false;
            bool allGamesDone = true;
            Ticket t = new Ticket();

            List<Ticket> tickets = new List<Ticket>(); //lista tiketa koji se brisu iz liste

            Object obj = Persistance.ReadFromFile("betUsers");
            Dictionary<string, User> betUsersFromFile = new Dictionary<string, User>();
            if (obj != null)
                betUsersFromFile = (Dictionary<string, User>)obj;


            obj = Persistance.ReadFromFile("results");
            Dictionary<int, Game> resultsFromFile = new Dictionary<int, Game>();
            if (obj != null)
                resultsFromFile = (Dictionary<int, Game>)obj;


            //    if (BetService.BetUsers.Count > 0 && BetService.Rezultati.Count > 0)
            if (betUsersFromFile.Count > 0 && resultsFromFile.Count > 0)
            {
                //  foreach (KeyValuePair<string, User> user in BetService.BetUsers)
                foreach (KeyValuePair<string, User> user in betUsersFromFile)

                {
                    if (user.Value.Tickets.Count > 0)
                    {
                        foreach (Ticket ticket in user.Value.Tickets)
                        {
                            if (ticket.Bets.Count > 0)
                            {
                                foreach (KeyValuePair<int, Game> bet in ticket.Bets)
                                {
                                    if (resultsFromFile.ContainsKey(bet.Key))
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
                            }
                        }

                        foreach (var item in tickets)
                        {
                            user.Value.Tickets.Remove(item);
                        }

                        User changeUser = user.Value;
                        BetService betService = new BetService();
                        betService.EditUser(Helper.ObjectToByteArray(changeUser));
                    }
                }
            }
            return true;
        }

        private static bool SendTicketResults2(User user, Ticket ticket)//sve utakmice na tiketu gotove, salje se ishod
        {
            Object obj = Persistance.ReadFromFile("results");
            Dictionary<int, Game> resultsFromFile = new Dictionary<int, Game>();
            if (obj != null)
                resultsFromFile = (Dictionary<int, Game>)obj;

            bool won = true;

            foreach (KeyValuePair<int, Game> bet in ticket.Bets)
            {
                bet.Value.HomeGoalScored = resultsFromFile[bet.Key].HomeGoalScored;
                bet.Value.AwayGoalScored = resultsFromFile[bet.Key].AwayGoalScored;


                if (resultsFromFile[bet.Key].Tip != bet.Value.Tip)
                {
                    bet.Value.Won = false;
                    won = false;
                }
                else
                    bet.Value.Won = true;
            }

            NetTcpBinding binding = new NetTcpBinding();

            string srvCertCN = "betserviceintegration";
            binding = new NetTcpBinding();
            binding.Security.Transport.ClientCredentialType = TcpClientCredentialType.Certificate;

            X509Certificate2 srvCert = CertManager.GetCertificateFromStorage(StoreName.My, StoreLocation.LocalMachine, srvCertCN);
            EndpointAddress address = new EndpointAddress(new Uri("net.tcp://localhost:" + Helper.integrationHostPort + "/ClientIntegrationPlatform"),
                                      new X509CertificateEndpointIdentity(srvCert));




            BetServerProxy proxy = new BetServerProxy(binding, address);
            {
                byte[] encryptedPort;
                encryptedPort = Helper.Encrypt(user.Port);

                if (proxy.CheckIfAlive(encryptedPort))//ako vrati false obrisati tog user-a?
                {
                    byte[] encryptedTicket = Helper.Encrypt(ticket);
                    byte[] encryptedWon = Helper.Encrypt(won);

                    proxy.SendTicketResults(encryptedTicket, encryptedWon, encryptedPort); // treba port od klijenta kom salje
                }
            }


            if (won)
            {
                User changeUser = user;
                BetService betService = new BetService();

                changeUser.BetAccount.Amount += ticket.CashPrize;
                betService.EditUser(Helper.ObjectToByteArray(changeUser));
            }
            return true;
        }



        private static bool SendGameResults()
        {
            List<Game> results = new List<Game>();
            List<int> gameIDs = new List<int>();
            BetOffer betOffer = new BetOffer();
            List<int> indexToDelete = new List<int>();

            List<int> finishedGame = new List<int>();

            int index;
            int home;
            int away;
            int tip;
            int offersNumber;
            int finished;
            int j;

            while (true)
            {
                Thread.Sleep(20000);

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

                        Object obj = Persistance.ReadFromFile("results");
                        Dictionary<int, Game> resultsFromFile = new Dictionary<int, Game>(); //citamo iz fajla rezultate
                        if (obj != null)
                            resultsFromFile = (Dictionary<int, Game>)obj;

                        resultsFromFile.Add(betOffer.Id, game); //dodajemo utakmicu u listu zavrsenih utakmica                           

                        Persistance.WriteToFile(resultsFromFile, "results"); //upisujemo u fajl

                        finishedGame.Add(betOffer.Id);

                        finished--;
                        j++;
                    } while (finished > 0);


                    lock (ResultsLock)
                    {

                        foreach (var item in finishedGame)
                        {
                            DeleteFinishedGame(item);
                        }


                        //saljemo svima rezultate gotovih utakmica
                        lock (BetService.PortLock)
                        {
                            string srvCertCN = "betserviceintegration";
                            NetTcpBinding binding = new NetTcpBinding();
                            binding.Security.Transport.ClientCredentialType = TcpClientCredentialType.Certificate;

                            X509Certificate2 srvCert = CertManager.GetCertificateFromStorage(StoreName.My, StoreLocation.LocalMachine, srvCertCN);
                            EndpointAddress address = new EndpointAddress(new Uri("net.tcp://localhost:" + Helper.integrationHostPort + "/ClientIntegrationPlatform"),
                                                      new X509CertificateEndpointIdentity(srvCert));

                            BetServerProxy proxy = new BetServerProxy(binding, address);
                            {
                                byte[] encryptedPort;
                                encryptedPort = Helper.Encrypt(Helper.clientPrintPort);

                                if (proxy.CheckIfAlive(encryptedPort))
                                {
                                    byte[] encryptedResults = Helper.Encrypt(results);
                                    proxy.SendGameResults(encryptedResults, encryptedPort); //treba i port da se salje
                                    sendOffers = true;
                                }
                            }
                        }

                        checkUserGames = true;
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


