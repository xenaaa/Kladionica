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
        public bool SendGameResults(List<string> results)
        {
            Console.WriteLine("Results:");
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

        public bool SendOffers(List<BetOffer> offers)        
        {
            Console.WriteLine("                   1   X   2");
            foreach (var item in offers)
            {
                Console.WriteLine("{0} {1} {2} {3} {4} {5}",item.Id,item.Home,item.Away,item.Odds[1],item.Odds[0],item.Odds[2]);
            }
            return true;
        }

        public bool SendTicketResults()
        {
            throw new NotImplementedException();
        }
    }
}
