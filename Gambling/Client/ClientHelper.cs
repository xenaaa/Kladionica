using IntegrationPlatform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace Client
{
    public class ClientHelper : IClientHelper
    {

        public static Dictionary<int, BetOffer> Offers = new Dictionary<int, BetOffer>();
        public bool SendGameResults(List<string> results)
        {
            Console.WriteLine("Results:\n");
            foreach (string str in results)
            {
                Console.WriteLine("Offer: {0}", str);
            }
            return true;
        }

        public bool CheckIfAlive()
        {
            return true;
        }

        public bool SendOffers(Dictionary<int, BetOffer> offers)
        {
            Offers = offers;
            Console.WriteLine("                   1   X   2");
            foreach (var item in offers)
            {           
                Console.WriteLine("{0} {1} {2} {3} {4} {5}", item.Value.Id, item.Value.Home, item.Value.Away, item.Value.Odds[1], item.Value.Odds[0], item.Value.Odds[2]);
            }
            return true;
        }

        public bool SendTicketResults(Ticket tiket, bool prosao)
        {
            double cashPrize = 1;

            if (prosao)
            {
                Console.WriteLine("                   TIKET DOBITNI");
                foreach (KeyValuePair<int, Game> item in tiket.Bets)
                {
                    //SVE ZELENO
                    Console.WriteLine("Sifra utakmic: {0}, tip: {1}\n", item.Key, item.Value);
                    cashPrize *= item.Value.Odds;
                }
                cashPrize *= tiket.Payment;
                Console.WriteLine("Dobitak: "+ cashPrize);
            }
            else
            {
                Console.WriteLine("                   TIKET GUBITNI");
                foreach (KeyValuePair<int, Game> item in tiket.Bets)
                {
                    if (item.Value.Won)//crvena boja
                        Console.WriteLine("Sifra utakmic: {0}, tip: {1}\n", item.Key, item.Value);
                    else //zelena boja
                        Console.WriteLine("Sifra utakmic: {0}, tip: {1}\n", item.Key, item.Value);
                }
            }
            return true;
        }
    }
}
