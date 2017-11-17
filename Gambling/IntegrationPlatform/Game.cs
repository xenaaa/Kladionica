using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationPlatform
{
    [DataContract]
    public  class Game
    {
        private bool won;
        private int tip;
        private double odds;

        [DataMember]
        public bool Won
        {
            get { return won; }
            set { won = value; }
        }


        [DataMember]
        public int Tip
        {
            get { return tip; }
            set { tip = value; }
        }


        [DataMember]
        public double Odds
        {
            get { return odds; }
            set { odds = value; }
        }
    }
}
