using Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientPrint
{
    public class ClientPrint : IClientHelper
    {
        public bool SendGameResults(List<Game> results, int port)
        {

            //Console.ForegroundColor = ConsoleColor.White;
            //Console.WriteLine("\n*******************Results:**********************\n");
            //foreach (string str in results)
            //{
            //    Console.WriteLine("{0}", str);
            //}
            //Console.WriteLine("***************************************************\n");


            //return true;



            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\n\n-------------------------------------RESULTS------------------------------------------------\n");
            Console.WriteLine("ID |       HOME        |       AWAY        |         RESULT        ");
            Console.WriteLine("--------------------------------------------------------------------------------------------");

            foreach (var item in results)
            {
                Console.WriteLine(String.Format("{0,-10}  {1,-10}     {2,-10}       {3,-2}   :    {4,-5} ", item.BetOffer.Id, item.BetOffer.Home, item.BetOffer.Away, item.HomeGoalScored, item.AwayGoalScored));
            }
            Console.WriteLine("**********************************************************************************************\n\n");
            Console.ForegroundColor = ConsoleColor.White;
            return true;
        }


        public bool CheckIfAlive(int port)
        {
            return true;
        }
        public bool SendOffers(Dictionary<int, BetOffer> offers, int port)
        {

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("--------------------------------------------------------------------------------------------");
            Console.WriteLine("ID |       HOME        |       AWAY        |       1       |       X       |       2       ");
            Console.WriteLine("--------------------------------------------------------------------------------------------");

            foreach (var item in offers)
            {
                Console.WriteLine(String.Format("{0,-10}  {1,-10}     {2,-10}              {3,-5}           {4,-5}           {5,-5}  ", item.Key, item.Value.Home, item.Value.Away, item.Value.Odds[1], item.Value.Odds[0], item.Value.Odds[2]));
            }
            Console.WriteLine("**********************************************************************************************"); ;

            return true;

        }

        public bool SendTicketResults(Ticket tiket, bool prosao, int port)
        {
            throw new NotImplementedException();
        }
    }
}
