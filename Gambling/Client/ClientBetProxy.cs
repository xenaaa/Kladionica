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
            throw new NotImplementedException();
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

        

        public bool SendTicketResults()
        {
            throw new NotImplementedException();
        }
        public bool SendTicket(Ticket ticket, string username)
        {
            bool sent = false;
            try
            {
                sent = factory.SendTicket(ticket,username);
                Console.WriteLine("SendTicket() >> {0}", sent);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error while trying to SendTicket(). {0}", e.Message);
            }

            return sent;
        }
    }
}
