using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Contracts
{
    [Serializable]
    [DataContract]
    public class BetOffer
    {
        private string away;
        private string home;
        private int id;
        private Dictionary<int, double> odds;

        public BetOffer() { }
        public BetOffer(string home, string away, int id, Dictionary<int, double> odds)
        {
            this.away = away;
            this.home = home;
            this.id = id;
            this.odds = odds;
        }


        [DataMember]
        public string Away
        {
            get { return away; }
            set { away = value; }
        }


        [DataMember]
        public string Home
        {
            get { return home; }
            set { home = value; }
        }


        [DataMember]
        public int Id
        {
            get { return id; }
            set { id = value; }
        }


        [DataMember]
        public Dictionary<int, double> Odds
        {
            get { return odds; }
            set { odds = value; }
        }


    }
}
