﻿using IntegrationPlatform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetServer
{
    public class BetService : IBetService
    {
        public bool AddUser(User user)
        {
            throw new NotImplementedException();
        }

        public bool DeleteUser(User user)
        {
            throw new NotImplementedException();
        }

        public bool EditUser(User user)
        {
            throw new NotImplementedException();
        }

        public bool SendGameResults(List<string> results)
        {
            throw new NotImplementedException();
        }

        //public bool SendOffers(List<BetOffer> offers)
        //{
        //    throw new NotImplementedException();
        //}

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
