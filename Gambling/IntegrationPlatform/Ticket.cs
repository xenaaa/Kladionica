using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationPlatform
{
    [DataContract]
    public class Ticket
    {
        //private Dictionary<int, int> bets = new Dictionary<int, int>();

        public Ticket(Dictionary<int, int> bets, int payment)
        {
            this.bets = bets;
            this.payment = payment;
        }

        //<sifra utakmice,tip>---npr <3001,1>
        private Dictionary<int, int> bets= new Dictionary<int, int>();

        [DataMember]
        public Dictionary<int, int> Bets
        {
            get { return bets; }
            set { bets = value; }
        }

    
        private int payment;

        [DataMember]
        public int Payment
        {
            get { return payment; }
            set { payment = value; }
        }


    }
}
