using IntegrationPlatform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace BetServer
{
    public class BetService : ChannelFactory<IBetService>,IBetService,IDisposable
    {
        IBetService factory;

        public BetService(NetTcpBinding binding, EndpointAddress address)
			: base(binding, address)
		{
            factory = this.CreateChannel();
        }
        private Dictionary<string, User> BetUsers = new Dictionary<string, User>();

        public bool AddUser(User user)
        {
            if(!BetUsers.ContainsKey(user.Username))
            {
                BetUsers.Add(user.Username, user);
                Console.WriteLine("User {0} successfully added to BetUsers", user.Username);
                return true;
            }
            else
            {

                Console.WriteLine("User {0} already exists.", user.Username);
                return false;
            }
        }

        public bool DeleteUser(User user)
        {
            if (!BetUsers.ContainsKey(user.Username))
            {
                Console.WriteLine("Error! There is no user {0} in BetService", user.Username);
                return false;   
            }
            else
            {
                BetUsers.Remove(user.Username);
                Console.WriteLine("User {0} removed from BetService", user.Username);
                return true;
            }
        }

        public bool EditUser(User user)
        {
            if (!BetUsers.ContainsKey(user.Username))
            {
                Console.WriteLine("Error! There is no user {0} in BetService", user.Username);
                return false;
            }
            else
            {
                foreach(KeyValuePair<string,User> kvp in BetUsers)
                {
                    if(kvp.Key == user.Username)
                    {
                        kvp.Value.BetAccount = user.BetAccount;
                        kvp.Value.Role = user.Role;
                        kvp.Value.Password = user.Password;
                    }
                }
                return true;
            }
        }

        public bool SendGameResults(List<string> results)
        {
            throw new NotImplementedException();
        }

        public bool SendOffers(List<BetOffer> offers)
        {
            bool sent = false;
            try
            {
                sent = factory.SendOffers(offers);
                Console.WriteLine("SendOffers() >> {0}",sent);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error while trying to SendOffers(). {0}", e.Message);
            }

            return sent;
            
        }

       

        public bool SendTicketResults()
        {
            throw new NotImplementedException();
        }

        public bool SendTicket(Ticket ticket, string username)//kladionica salje klijentu
        {

            if (BetUsers.ContainsKey(username))
            {
                BetUsers[username].Tickets.Add(ticket);
            }
            else
                return false;

            return true;

        }
    }
}
