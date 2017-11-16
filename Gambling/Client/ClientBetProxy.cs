using BetServer;
using IntegrationPlatform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    public class ClientBetProxy : ChannelFactory<IBetService>, IBetService, IDisposable
    {
        IBetService factory;

        public ClientBetProxy(NetTcpBinding binding, string address) : base(binding, address)
        {
            factory = this.CreateChannel();
        }

        public bool AddUser(User user)
        {
            try
            {
                factory.AddUser(user);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error {0}", e.Message);
                return false;
            }
        }

        public bool DeleteUser(User user)
        {
            try
            {
                factory.DeleteUser(user);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error {0}", e.Message);
                return false;
            }
        }

        public bool EditUser(User user)
        {
            try
            {
                factory.EditUser(user);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error {0}", e.Message);
                return false;
            }
        }

        public bool SendGameResults(List<string> results)
        {
            Console.WriteLine("Results:");
            foreach (string str in results)
            {
                Console.WriteLine("Offer: {0}", str);
            }
            return true;
        }

      

        public bool SendOffers(List<BetOffer> offers)
        {
            Console.WriteLine("Offers:");
            foreach (BetOffer offer in offers)
            {
                Console.WriteLine("Offer: {0}", offer);
            }
            return true;
        }

        public bool SendTicket(Ticket ticket, string username)
        {
            throw new NotImplementedException();
        }

        public bool SendTicketResults()
        {
            throw new NotImplementedException();
        }
    }
}
