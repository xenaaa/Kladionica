using IntegrationPlatform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace BetServer
{
    public class BetServerProxy : ChannelFactory<IClientHelper>, IClientHelper, IDisposable
    {
        IClientHelper factory;

        public BetServerProxy(NetTcpBinding binding, string address) : base(binding, address)
        {
            factory = this.CreateChannel();
            
        }

        public bool CheckIfAlive()
        {
            try
            {
                factory.CheckIfAlive();
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
            try
            {
                factory.SendGameResults(results);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error {0}", e.Message);
                return false;
            }

        }

        public bool SendOffers(List<BetOffer> offers)
        {
            try
            {
                factory.SendOffers(offers);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error {0}", e.Message);
                return false;
            }
        }

        public bool SendTicketResults(Ticket tiket1, bool prosao)
        {
            try
            {


                if (BetService.BetUsers.Count > 0 && BetService.Rezultati.Count > 0)
                {
                    foreach (KeyValuePair<string, User> user in BetService.BetUsers)
                    {
                        foreach (Ticket tiket in user.Value.Tickets)
                        {
                            if (tiket.Bets.Count > 0)
                                foreach (KeyValuePair<int, int> bet in tiket.Bets)
                                {
                                    if (BetService.Rezultati.ContainsKey(bet.Key))//ne sme biti prazan tiket
                                    {
                                        if (!BetService.Rezultati[bet.Key].ContainsKey(bet.Value))
                                        {
                                            factory.SendTicketResults(tiket, false);
                                            break;
                                        }

                                    }
                                }
                            else
                                continue;

                            factory.SendTicketResults(tiket,true);
                        }
                        user.Value.Tickets.Clear();
                    }
                }






               
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error {0}", e.Message);
                return false;
            }
        }
    }
}