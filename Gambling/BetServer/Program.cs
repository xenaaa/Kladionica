using BetServer;
using CertificateManager;
using Contracts;
using SecurityManager;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
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
        static int FreeTcpPort()
        {
            TcpListener l = new TcpListener(IPAddress.Loopback, 0);
            l.Start();
            int port = ((IPEndPoint)l.LocalEndpoint).Port;
            l.Stop();
            return port;
        }

        static void Main(string[] args)
        {
            Persistance.EmptyBetFiles();
            string srvCertCN = "betservice";

            NetTcpBinding binding = new NetTcpBinding();
            binding.Security.Transport.ClientCredentialType = TcpClientCredentialType.Certificate;

            int port = FreeTcpPort();

            string address = "net.tcp://localhost:" + port + "/BetService";

            ServiceHost host = new ServiceHost(typeof(BetService));
            host.AddServiceEndpoint(typeof(IBetService), binding, address);


            //sertifikacija
            host.Credentials.ClientCertificate.Authentication.CertificateValidationMode = X509CertificateValidationMode.ChainTrust;
            host.Credentials.ClientCertificate.Authentication.RevocationMode = X509RevocationMode.NoCheck;

            ///Set appropriate service's certificate on the host. Use CertManager class to obtain the certificate based on the "srvCertCN"
            host.Credentials.ServiceCertificate.Certificate = CertManager.GetCertificateFromStorage(StoreName.My, StoreLocation.LocalMachine, srvCertCN);

            try
            {
                host.Open();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            string srvCertCN2 = "betserviceintegration";
            binding = new NetTcpBinding();
            binding.Security.Transport.ClientCredentialType = TcpClientCredentialType.Certificate;

            X509Certificate2 srvCert = CertManager.GetCertificateFromStorage(StoreName.TrustedPeople, StoreLocation.LocalMachine, srvCertCN2);
            EndpointAddress address2 = new EndpointAddress(new Uri("net.tcp://" + Helper.integrationHostAddress + ":" + Helper.integrationHostPort + "/ClientIntegrationPlatform"),
                                      new X509CertificateEndpointIdentity(srvCert));

            BetServerProxy proxy;
            // proxy = new BetServerProxy(binding, address2);


            string IP = string.Empty;
            var hostIP = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in hostIP.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    IP = ip.ToString();
                }
            }

            address = address.Replace("localhost", IP);


            while (true)
            {
                proxy = new BetServerProxy(binding, address2);
                if (proxy.GetServiceIP(Helper.Encrypt(address)))
                {
                    proxy.Close();
                    break;
                }
                else
                {
                    Console.WriteLine("Server not responding!");
                    proxy.Abort();
                    Thread.Sleep(1000);
                    continue;
                }
            }




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

            new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                CheckUserGames();
            }).Start();

            //while (true)
            //{

            //    if (checkUserGames)
            //        CheckUserGames();
            //    else
            //        Thread.Sleep();
            //}

            Console.WriteLine("Bet service is started.");
            Console.WriteLine("Press <enter> to stop service...");

            Console.ReadLine();
            host.Close();//nece se host zatvoriti
        }

        private static void SendOffers() //slanje ponude kijentu svakih 5 minuta
        {
            NetTcpBinding binding = new NetTcpBinding();

            XmlDocument xmlDoc = new XmlDocument(); // Create an XML document object

            while (true)
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
                    Thread.Sleep(200);
                } while (start.AddSeconds(15) > DateTime.Now);

                sendOffers = false;

                lock (XMLLock)
                {
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

                                X509Certificate2 srvCert = CertManager.GetCertificateFromStorage(StoreName.TrustedPeople, StoreLocation.LocalMachine, srvCertCN);
                                EndpointAddress address = new EndpointAddress(new Uri("net.tcp://" + Helper.integrationHostAddress + ":" + Helper.integrationHostPort + "/ClientIntegrationPlatform"),
                                                          new X509CertificateEndpointIdentity(srvCert));

                                BetServerProxy proxy;

                                proxy = new BetServerProxy(binding, address);

                                byte[] encryptedPort;
                                byte[] encryptedPrintPort;
                                byte[] encryptedAddress;

                                Dictionary<string, User> usersFromFile = new Dictionary<string, User>();
                                Object obj = Persistance.ReadFromFile("betUsers.txt");
                                if (obj != null)
                                    usersFromFile = (Dictionary<string, User>)obj;
                                List<string> adresses = new List<string>();

                                byte[] encryptedOffers = Helper.Encrypt(Offers);

                                foreach (KeyValuePair<string, User> user in usersFromFile)
                                {
                                    if (!string.IsNullOrEmpty(user.Value.Address))
                                    {
                                        encryptedPort = Helper.Encrypt(user.Value.Port);
                                        encryptedAddress = Helper.Encrypt(user.Value.Address);

                                        if (!adresses.Contains(user.Value.Address))
                                        {
                                            encryptedPrintPort = Helper.Encrypt(user.Value.PrintPort);
                                            if (proxy.CheckIfAlive(encryptedPrintPort, encryptedAddress, Helper.Encrypt(true)))
                                            {
                                                proxy.SendOffers(encryptedOffers, encryptedPrintPort, encryptedAddress, Helper.Encrypt(true));

                                            }
                                            adresses.Add(user.Value.Address);
                                        }
                                        if (proxy.CheckIfAlive(encryptedPort, encryptedAddress, Helper.Encrypt(false)))
                                        {
                                            proxy.SendOffers(encryptedOffers, encryptedPort, encryptedAddress, Helper.Encrypt(false));
                                        }
                                    }
                                    else
                                        continue;
                                }
                            }
                        }
                    }
                }
            }
        }


        private static bool CheckUserGames()//svake 2 sekunde proverava da li su se zavrsile sve utakmice na tiketima za svakog User-a pojedinacno. Ako su sve utakmice na tiketu zavrsene tikes se salje na proveru i brise
        {
            while (true)
            {
                DateTime start = DateTime.Now;
                //   DateTime now;

                while (true)
                {
                    if (checkUserGames)
                    {
                        break;
                    }
                    Thread.Sleep(200);
                }


                checkUserGames = false;
                bool allGamesDone = true;
                Ticket t = new Ticket();

                List<Ticket> tickets = new List<Ticket>(); //lista tiketa koji se brisu iz liste

                Object obj = Persistance.ReadFromFile("betUsers.txt");
                Dictionary<string, User> betUsersFromFile = new Dictionary<string, User>();
                if (obj != null)
                    betUsersFromFile = (Dictionary<string, User>)obj;


                obj = Persistance.ReadFromFile("results.txt");
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
                                allGamesDone = true;
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
                                    tickets.Add(ticket); //
                                }
                            }

                            bool delete = false;

                            foreach (var item in tickets)
                            {
                                delete = true;
                                user.Value.Tickets.Remove(item);
                            }
                            if (delete)
                            {
                                User changeUser = user.Value;
                                BetService betService = new BetService();
                                betService.EditUser(Helper.Encrypt(changeUser), Helper.ObjectToByteArray(0)); // ne znam sta ovdje proslijediti?????
                            }
                        }

                    }

                }
            }
            return true;
        }

        private static bool SendTicketResults2(User user, Ticket ticket)//sve utakmice na tiketu gotove, salje se ishod
        {
            Object obj = Persistance.ReadFromFile("results.txt");
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

            X509Certificate2 srvCert = CertManager.GetCertificateFromStorage(StoreName.TrustedPeople, StoreLocation.LocalMachine, srvCertCN);
            EndpointAddress address = new EndpointAddress(new Uri("net.tcp://" + Helper.integrationHostAddress + ":" + Helper.integrationHostPort + "/ClientIntegrationPlatform"),
                                      new X509CertificateEndpointIdentity(srvCert));

            BetServerProxy proxy = new BetServerProxy(binding, address);

            byte[] encryptedPort, encryptedAddress;
            encryptedPort = Helper.Encrypt(user.Port);
            encryptedAddress = Helper.Encrypt(user.Address);

            if (proxy.CheckIfAlive(encryptedPort, encryptedAddress, Helper.Encrypt(false)))//ako vrati false obrisati tog user-a?
            {
                byte[] encryptedTicket = Helper.Encrypt(ticket);
                byte[] encryptedWon = Helper.Encrypt(won);

                proxy.SendTicketResults(encryptedTicket, encryptedWon, encryptedPort, encryptedAddress); // treba port od klijenta kom salje
            }


            if (won)//koja je svrha
            {
                User changeUser = user;
                BetService betService = new BetService();
                changeUser.BetAccount.Amount += ticket.CashPrize;
                betService.EditUser(Helper.Encrypt(changeUser), Helper.ObjectToByteArray(0));
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

            int index, home, away, tip, offersNumber, finished, j;

            Dictionary<string, User> usersFromFile = new Dictionary<string, User>();
            Object obj;
            while (true)
            {
                Thread.Sleep(20000);

                obj = Persistance.ReadFromFile("betUsers.txt");
                if (obj != null)
                    usersFromFile = (Dictionary<string, User>)obj;
                if (usersFromFile.Values.Any(x => (x.Role == "User" || x.Role == "Reader") && !string.IsNullOrEmpty(x.Address)))
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
                        finished = r.Next(1, 8);  //broj utakmica koje ce se zavrsiti
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
                        lock (XMLLock)
                        {
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

                                obj = Persistance.ReadFromFile("results.txt");
                                Dictionary<int, Game> resultsFromFile = new Dictionary<int, Game>(); //citamo iz fajla rezultate
                                if (obj != null)
                                    resultsFromFile = (Dictionary<int, Game>)obj;

                                resultsFromFile.Add(betOffer.Id, game); //dodajemo utakmicu u listu zavrsenih utakmica                           

                                Persistance.WriteToFile(resultsFromFile, "results.txt"); //upisujemo u fajl

                                finishedGame.Add(betOffer.Id);

                                finished--;
                                j++;
                            } while (finished > 0);



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

                                X509Certificate2 srvCert = CertManager.GetCertificateFromStorage(StoreName.TrustedPeople, StoreLocation.LocalMachine, srvCertCN);
                                EndpointAddress address = new EndpointAddress(new Uri("net.tcp://" + Helper.integrationHostAddress + ":" + Helper.integrationHostPort + "/ClientIntegrationPlatform"),
                                                          new X509CertificateEndpointIdentity(srvCert));

                                BetServerProxy proxy = new BetServerProxy(binding, address);

                                byte[] encryptedPort, encryptedAddress, encryptedPrintPort;

                                List<string> adresses = new List<string>();
                                byte[] encryptedOffers = Helper.Encrypt(results);
                                foreach (KeyValuePair<string, User> user in usersFromFile)
                                {
                                    if (!string.IsNullOrEmpty(user.Value.Address))
                                    {
                                        if (!adresses.Contains(user.Value.Address))
                                        {
                                            encryptedPort = Helper.Encrypt(user.Value.Port);
                                            encryptedAddress = Helper.Encrypt(user.Value.Address);
                                            encryptedPrintPort = Helper.Encrypt(user.Value.PrintPort);
                                            if (proxy.CheckIfAlive(encryptedPrintPort, encryptedAddress, Helper.Encrypt(true)))
                                            {
                                                proxy.SendGameResults(encryptedOffers, encryptedPrintPort, encryptedAddress);

                                            }
                                            adresses.Add(user.Value.Address);
                                            sendOffers = true;
                                        }

                                    }
                                    else
                                        continue;
                                }
                                checkUserGames = true;



                            }

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