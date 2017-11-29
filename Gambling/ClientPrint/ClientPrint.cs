using Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientPrint
{
    public class ClientPrint : IClientPrint
    {
        public bool SendGameResults(byte[] resultsBytes, byte[] port, byte[] address)
        {
            List<Game> results = (List<Game>)Helper.ByteArrayToObject(resultsBytes);

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\n\n-------------------------------------RESULTS------------------------------------------------\n");
            Console.WriteLine("ID |       HOME        |       AWAY        |         RESULT        ");
            Console.WriteLine("--------------------------------------------------------------------------------------------");

            foreach (var item in results)
            {
                Console.WriteLine(String.Format("{0,-10}  {1,-10}     {2,-10}                 {3,-2}: {4,-5} ", item.BetOffer.Id, item.BetOffer.Home, item.BetOffer.Away, item.HomeGoalScored, item.AwayGoalScored));
            }
            Console.WriteLine("**********************************************************************************************\n\n");
            Console.ForegroundColor = ConsoleColor.White;
            return true;
        }


        public bool CheckIfAlive(byte[] port, byte[] address, byte[] isItPrintClientBytes)
        {
            return true;
        }
        public bool SendOffers(byte[] offersBytes, byte[] portBytes, byte[] addressBytes, byte[] isItPrintClientBytes)
        {

            Dictionary<int, BetOffer> offers = (Dictionary<int, BetOffer>)Helper.ByteArrayToObject(offersBytes);

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("------------------------------------DAILY OFFER------------------------------------------------\n");
            Console.WriteLine("ID |       HOME        |       AWAY        |       1       |       X       |       2       ");
            Console.WriteLine("--------------------------------------------------------------------------------------------");

            foreach (var item in offers)
            {
                Console.WriteLine(String.Format("{0,-10}  {1,-10}     {2,-10}              {3,-5}           {4,-5}           {5,-5}  ", item.Key, item.Value.Home, item.Value.Away, item.Value.Odds[1], item.Value.Odds[0], item.Value.Odds[2]));
            }
            Console.WriteLine("**********************************************************************************************\n\n"); ;

            return true;

        }

        public bool CloseProxy()
        {
            return true;
        }
    }
}