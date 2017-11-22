using Contracts;
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


        public bool SendGameResults(byte[] results, byte[] port)
        {
            //if (Monitor.TryEnter(PrintLock))
            //{
            //    lock (PrintLock)
            //    {
            //        Console.ForegroundColor = ConsoleColor.White;
            //        Console.WriteLine("\n*******************Results:**********************\n");
            //        foreach (string str in results)
            //        {
            //            Console.WriteLine("{0}", str);
            //        }
            //        Console.WriteLine("***************************************************\n");
            //    }
            //    Monitor.Exit(PrintLock);

            //}

            return true;
        }

        public bool CheckIfAlive(byte[] port)
        {
            return true;
        }

        public bool SendOffers(byte[] offersBytes, byte[] port)
        {

            Dictionary<int, BetOffer> offers = (Dictionary<int, BetOffer>)Helper.ByteArrayToObject(offersBytes);

            if (Monitor.TryEnter(PrintLock))
            {
                //lock (PrintLock)
                //{
                //    Console.ForegroundColor = ConsoleColor.White;
                //    Console.WriteLine("--------------------------------------------------------------------------------------------");
                //    Console.WriteLine("ID |       HOME        |       AWAY        |       1       |       X       |       2       ");
                //    Console.WriteLine("--------------------------------------------------------------------------------------------");

                //    foreach (var item in offers)
                //    {
                //        Console.WriteLine(String.Format("{0,-10}  {1,-10}     {2,-10}              {3,-5}           {4,-5}           {5,-5}  ", item.Key, item.Value.Home, item.Value.Away, item.Value.Odds[1], item.Value.Odds[0], item.Value.Odds[2]));
                //    }
                //    Console.WriteLine("**********************************************************************************************");
                //    Console.WriteLine("Press Enter for new ticket");
                //}
                Monitor.Exit(PrintLock);
                Offers = offers;
                return true;
            }

            return false;
        }

        public bool SendTicketResults(byte[] ticketBytes, byte[] isPassedBytes, byte[] portBytes)
        {

            Ticket ticket = (Ticket)Helper.ByteArrayToObject(ticketBytes);

            bool isPassed = (bool)Helper.ByteArrayToObject(isPassedBytes);


            if (isPassed)
            {
                if (Monitor.TryEnter(PrintLock))
                {
                    lock (PrintLock)
                    {
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.WriteLine("\n**********************************TICKET WON************************************\n");
                        Console.WriteLine("-----------------------------------------------------------------------------------");
                        Console.WriteLine("ID |       HOME        |       AWAY        |       RESULT      |       TIP       ");
                        Console.WriteLine("-----------------------------------------------------------------------------------");

                        foreach (KeyValuePair<int, Game> item in ticket.Bets)
                        {
                            //SVE ZELENO
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine(String.Format("{0,-10} {1,-10}          {2,-10}             {3,-1} : {4,-4}           {5,-5}  ", item.Key, item.Value.BetOffer.Home, item.Value.BetOffer.Away, item.Value.HomeGoalScored, item.Value.AwayGoalScored, item.Value.Tip));
                            //   Console.ForegroundColor = ConsoleColor.White;
                        }

                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("\nWin: " + ticket.CashPrize);
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.WriteLine("\n*********************************************************************************");



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
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.WriteLine("\n**********************************TICKET LOST************************************\n");
                        Console.WriteLine("-----------------------------------------------------------------------------------");
                        Console.WriteLine("ID |       HOME        |       AWAY        |       RESULT      |       TIP       ");
                        Console.WriteLine("-----------------------------------------------------------------------------------");

                        foreach (KeyValuePair<int, Game> item in ticket.Bets)
                        {
                            if (item.Value.Won)//zelena boja
                            {
                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.WriteLine(String.Format("{0,-10} {1,-10}          {2,-10}           {3,-1} : {4,-4}           {5,-5}  ", item.Key, item.Value.BetOffer.Home, item.Value.BetOffer.Away, item.Value.HomeGoalScored, item.Value.AwayGoalScored, item.Value.Tip));
                            }
                            else //crvena boja
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine(String.Format("{0,-10} {1,-10}          {2,-10}           {3,-1} : {4,-4}           {5,-5}  ", item.Key, item.Value.BetOffer.Home, item.Value.BetOffer.Away, item.Value.HomeGoalScored, item.Value.AwayGoalScored, item.Value.Tip));
                            }
                        }
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.WriteLine("\nWin: 0");
                        Console.WriteLine("**********************************************************************************"); ;
                    }
                    Monitor.Exit(PrintLock);
                }

            }
            return true;
        }
    }
}
