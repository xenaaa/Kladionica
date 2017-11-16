using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationPlatform
{
    [DataContract]
    public class BetOffer
    {
        public BetOffer(string away,string home,int id, Dictionary<int, double> odds)
        {
            this.away = away;
            this.home = home;
            this.id = id;
            this.odds = odds;
        }

        private string away;
        [DataMember]
        public string Away
        {
            get { return away; }
            set { away = value; }
        }

        private string home;
        [DataMember]
        public string Home
        {
            get { return home; }
            set { home = value; }
        }

        private int id;
        [DataMember]
        public int Id
        {
            get { return id; }
            set { id = value; }
        }

        private Dictionary<int ,double > odds;
        [DataMember]
        public Dictionary<int ,double > Odds
        {
            get { return odds; }
            set { odds = value; }
        }


    }
}
