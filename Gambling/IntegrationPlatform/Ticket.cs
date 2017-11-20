using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Contracts
{


    [DataContract]
    public class Ticket
    {
        private Dictionary<int, Game> bets;
        private int payment;
        private double cashPrize;

        public Ticket()
        {
            bets = new Dictionary<int, Game>();
            this.cashPrize = 1;

            foreach (var item in bets)
            {
                int tip = item.Value.Tip;
                this.cashPrize *= item.Value.BetOffer.Odds[tip];
            }

            this.cashPrize *= this.payment;
        }

        public Ticket(Dictionary<int, Game> bets, int payment)
        {
            this.bets = bets;
            this.payment = payment;
            this.cashPrize = 1;

            foreach (var item in bets)
            {
                int tip = item.Value.Tip;
                this.cashPrize *= item.Value.BetOffer.Odds[tip];
            }

            this.cashPrize *= this.payment;
        }

        [DataMember]
        public Dictionary<int, Game> Bets
        {
            get { return bets; }
            set { bets = value; }
        }

        [DataMember]
        public int Payment
        {
            get { return payment; }
            set { payment = value; }
        }



        [DataMember]
        public double CashPrize
        {
            get { return cashPrize; }
            set { cashPrize = value; }
        }

    }
}
