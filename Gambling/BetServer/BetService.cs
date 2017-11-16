using IntegrationPlatform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace BetServer
{
    public class BetService : ChannelFactory<IBetService>, IBetService, IDisposable
    {
        IBetService factory;

        public BetService(NetTcpBinding binding, EndpointAddress address)
            : base(binding, address)
        {
            factory = this.CreateChannel();
        }


        private Dictionary<int, Dictionary<int, double>> Rezultati = new Dictionary<int, Dictionary<int, double>>();//puni se posle svakih 5 minuta, ondnosno puni se rezultatima gotovih utakmica


        private Dictionary<string, User> BetUsers = new Dictionary<string, User>();

        public bool AddUser(User user)
        {
            if (!BetUsers.ContainsKey(user.Username))
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
                foreach (KeyValuePair<string, User> kvp in BetUsers)
                {
                    if (kvp.Key == user.Username)
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
            bool sent = false;
            try
            {
                sent = factory.SendGameResults(results);
                Console.WriteLine("SendGameResults() >> {0}", sent);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error while trying to SendGameResults(). {0}", e.Message);
            }

            return sent;
        }

        public bool SendOffers(List<BetOffer> offers)
        {
            bool sent = false;
            try
            {
                sent = factory.SendOffers(offers);
                Console.WriteLine("SendOffers() >> {0}", sent);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error while trying to SendOffers(). {0}", e.Message);
            }

            return sent;

        }

        public bool SendTicketResults()
        {
            //if (BetUsers.Count > 0 && Rezultati.Count > 0)
            //{
            //    foreach (KeyValuePair<int, Dictionary<int, double>> offer in Rezultati)
            //    {
            //        foreach (KeyValuePair<string, User> user in BetUsers)
            //        {
            //            if (user.Value.Tickets.Count > 0)
            //            {
            //                foreach (Ticket tiket in user.Value.Tickets)
            //                {
            //                    foreach (KeyValuePair<int, int> bet in tiket.Bets)
            //                        if (offer.Value.ContainsKey(bet.Value) && bet.Key==)
            //                        {
            //                            factory.SendTicketResults();
            //                        }
            //                }
            //            }
            //        }
            //    }
            //}
            if (BetUsers.Count > 0 && Rezultati.Count > 0)
            {
                foreach (KeyValuePair<string, User> user in BetUsers)
                {
                    foreach (Ticket tiket in user.Value.Tickets)
                    {
                        foreach (KeyValuePair<int, int> bet in tiket.Bets)
                        {
                            if (Rezultati.ContainsKey(bet.Key))//ne sme biti prazan tiket
                            {
                                if(!Rezultati[bet.Key].ContainsKey(bet.Value))
                                    return false;
                                
                            }
                        }
                        factory.SendTicketResults();
                    }
                }
            }




                return true;
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
