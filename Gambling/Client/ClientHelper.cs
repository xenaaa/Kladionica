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
                        Console.WriteLine("{0}", str);
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
                    Console.WriteLine("                             1   X   2");
                    foreach (var item in offers)
                    {
                        Console.WriteLine("{0}  {1}     {2}     {3}     {4}     {5}  ",item.Key,item.Value.Home,item.Value.Away,item.Value.Odds[1], item.Value.Odds[0],item.Value.Odds[2]);                                        
                    }
                    Console.WriteLine("*******************************************************");
                    Console.WriteLine("Press Enter for new ticket");
                }
                Monitor.Exit(PrintLock);
                Offers = offers;
                return true;
            }
          
            return false;
        }

        public bool SendTicketResults(Ticket tiket, bool prosao, List<string> results)
        {
            int counter = 0;
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
                            Console.WriteLine("{0}     {1}  :  {2}      {3}      -   {4}  \n", item.Key, item.Value.BetOffer.Home, item.Value.BetOffer.Away,results[counter], item.Value.Tip);
                            counter++;
                        }

                        Console.WriteLine("Dobitak: " + tiket.CashPrize);
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
                                Console.WriteLine("{0}     {1}  :  {2}      {3}      -     {4}  \n", item.Key, item.Value.BetOffer.Home, item.Value.BetOffer.Away, results[counter], item.Value.Tip);
                            else //zelena boja
                                Console.WriteLine("{0}     {1}  :  {2}      {3}      -     {4}  \n", item.Key, item.Value.BetOffer.Home, item.Value.BetOffer.Away, results[counter], item.Value.Tip);
                            counter++;
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
