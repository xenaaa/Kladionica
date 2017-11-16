using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationPlatform
{
    public class Ticket
    {
        //private Dictionary<int, int> bets = new Dictionary<int, int>();

        private Dictionary<int, int> bets;

        public Dictionary<int, int> Bets
        {
            get { return bets; }
            set { bets = value; }
        }

    
        private int payment;

        public int Payment
        {
            get { return payment; }
            set { payment = value; }
        }


    }
}
