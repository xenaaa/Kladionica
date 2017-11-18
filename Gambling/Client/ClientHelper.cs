using IntegrationPlatform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace Client
{
    public class ClientHelper : IClientHelper
    {

        public static Dictionary<int, BetOffer> Offers = new Dictionary<int, BetOffer>();

        public static object printLock = new object();

        public static object PrintLock
        {
            get { return printLock; }
            set { printLock = value; }
        }


        public bool SendGameResults(List<string> results)
        {
            if (Monitor.TryEnter(PrintLock))
            {
                lock (PrintLock)
                {
                    Console.WriteLine("***************Results:***************\n");
                    foreach (string str in results)
                    {
                        Console.WriteLine("Offer: {0}", str);
                    }
                    Console.WriteLine("**************************************\n");
                }
                Monitor.Exit(PrintLock);

            }
           
            return true;
        }

        public bool CheckIfAlive()
        {
            return true;
        }

        public bool SendOffers(Dictionary<int, BetOffer> offers)
        {

            if (Monitor.TryEnter(PrintLock))
            {
                lock (PrintLock)
                {
                    Console.WriteLine("\n*******************************************************");
                    //Console.WriteLine("                   1   X   2");
                    foreach (var item in offers)
                    {
                        Console.WriteLine("\t-------------------------------------------------------");
                        Console.WriteLine("\t**{0}**\n\t\tHome: {1}\n\t\tAway: {2}", item.Value.Id, item.Value.Home, item.Value.Away);
                        //Console.WriteLine("\t\t      ");
                        Console.WriteLine("\t\t|1|: {0}   |x|: {1}   |2|: {2}", item.Value.Odds[1], item.Value.Odds[0], item.Value.Odds[2]);
                        

                    }
                    Console.WriteLine("\t-------------------------------------------------------");
                    Console.WriteLine("*******************************************************");
                    Console.WriteLine("Press Enter for new ticket");
                }
                Monitor.Exit(PrintLock);
                Offers = offers;
                return true;
            }
          
            return false;
        }

        public bool SendTicketResults(Ticket tiket, bool prosao)
        {
            double cashPrize = 1;

            if (prosao)
            {
                if (Monitor.TryEnter(PrintLock))
                {
                    lock (PrintLock)
                    {
                        Console.WriteLine("\n*********************TIKET DOBITNI*********************");
                        foreach (KeyValuePair<int, Game> item in tiket.Bets)
                        {
                            //SVE ZELENO
                            Console.WriteLine("Sifra utakmic: {0}, tip: {1}\n", item.Key, item.Value.Tip);
                            cashPrize *= item.Value.Odds;
                        }
                        cashPrize *= tiket.Payment;
                        Console.WriteLine("Dobitak: " + cashPrize);
                        Console.WriteLine("\n*******************************************************");
                    }
                    Monitor.Exit(PrintLock);
                }
                
            }
            else
            {
                if (Monitor.TryEnter(PrintLock))
                {
                    lock (PrintLock)
                    {
                        Console.WriteLine("\n*********************TIKET GUBITNI*********************");
                        
                        foreach (KeyValuePair<int, Game> item in tiket.Bets)
                        {
                            if (item.Value.Won)//crvena boja
                                Console.WriteLine("Sifra utakmic: {0}, tip: {1}\n", item.Key, item.Value.Tip);
                            else //zelena boja
                                Console.WriteLine("Sifra utakmic: {0}, tip: {1}\n", item.Key, item.Value.Tip);
                        }
                        Console.WriteLine("\n*******************************************************");
                    }
                    Monitor.Exit(PrintLock);
                }
               
            }
            return true;
        }
    }
}
