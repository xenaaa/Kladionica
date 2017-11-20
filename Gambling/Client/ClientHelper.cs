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
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine("\n*******************Results:**********************\n");
                    foreach (string str in results)
                    {
                        Console.WriteLine("{0}", str);
                    }
                    Console.WriteLine("***************************************************\n");
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
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine("--------------------------------------------------------------------------------------------");
                    Console.WriteLine("ID |       HOME        |       AWAY        |       1       |       X       |       2       ");
                    Console.WriteLine("--------------------------------------------------------------------------------------------");

                    foreach (var item in offers)
                    {
                        Console.WriteLine(String.Format("{0,-10}  {1,-10}     {2,-10}              {3,-5}           {4,-5}           {5,-5}  ", item.Key, item.Value.Home, item.Value.Away, item.Value.Odds[1], item.Value.Odds[0], item.Value.Odds[2]));
                    }

                    //Console.WriteLine(String.Format("{0,-10} | {1,-10} | {2,5}", "Bill", "Gates", 51));
                    //Console.WriteLine(String.Format("{0,-10} | {1,-10} | {2,5}", "Edna", "Parker", 114));
                    //Console.WriteLine(String.Format("{0,-10} | {1,-10} | {2,5}", "Johnny", "Depp", 44));
                    //Console.WriteLine("-------------------------------");


                    //Console.ForegroundColor = ConsoleColor.White;
                    //Console.WriteLine("\n*******************************************************");
                    //Console.WriteLine("                             1   X   2");
                    //foreach (var item in offers)
                    //{
                    //    Console.WriteLine("{0}  {1}     {2}     {3}     {4}     {5}  ",item.Key,item.Value.Home,item.Value.Away,item.Value.Odds[1], item.Value.Odds[0],item.Value.Odds[2]);                                        
                    //}
                    Console.WriteLine("**********************************************************************************************");
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
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine("{0}     {1}  :  {2}      {3}      -   {4}  \n", item.Key, item.Value.BetOffer.Home, item.Value.BetOffer.Away,results[counter], item.Value.Tip);
                            Console.ForegroundColor = ConsoleColor.White;
                            counter++;
                        }

                        Console.ForegroundColor = ConsoleColor.Yellow;
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
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine("\n*********************TIKET GUBITNI*********************");

                        foreach (KeyValuePair<int, Game> item in tiket.Bets)
                        {
                            if (item.Value.Won)//zelena boja
                            {
                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.WriteLine("{0}     {1}  :  {2}      {3}      -     {4}  \n", item.Key,item.Value.BetOffer.Home,item.Value.BetOffer.Away,results[counter],item.Value.Tip);
                            }
                            else //crvena boja
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("{0}     {1}  :  {2}      {3}      -     {4}  \n", item.Key, item.Value.BetOffer.Home, item.Value.BetOffer.Away, results[counter], item.Value.Tip);
                               
                            }
                            Console.ForegroundColor = ConsoleColor.White;
                            counter++;

                        }
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine("*******************************************************");
                    }
                    Monitor.Exit(PrintLock);
                }
               
            }
            return true;
        }
    }
}
