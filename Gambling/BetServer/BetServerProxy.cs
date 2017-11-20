﻿using Contracts;
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

        public bool CheckIfAlive(int port)
        {
            try
            {
                factory.CheckIfAlive(port);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error {0}", e.Message);
                return false;
            }

        }
        public bool SendGameResults(List<Game> results,int port)
        {
            try
            {
                factory.SendGameResults(results,port);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error {0}", e.Message);
                return false;
            }

        }

        public bool SendOffers(Dictionary<int,BetOffer> offers, int port)
        {
            try
            {
                factory.SendOffers(offers,port);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error {0}", e.Message);
                return false;
            }
        }

        public bool SendTicketResults(Ticket ticket, bool isPassed,  int port)
        {
            try
            {
                factory.SendTicketResults(ticket, isPassed, port);
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