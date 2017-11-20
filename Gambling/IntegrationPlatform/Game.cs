using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Contracts
{
    [DataContract]
    public class Game
    {
        private BetOffer betOffer;
        //private int homeGoalScored;
        //private int awayGoalScored;
        private string result;
        private int tip;
        private bool won;

        public Game()
        {
            //  betOffer = new BetOffer();
            //  homeGoalScored = 0;
            // awayGoalScored = 0;
            // won = false;
        }

        public Game(BetOffer betOffer, string result, int tip)
        {
            this.betOffer = betOffer;
            this.result = result;
            //homeGoalScored = home;
            //awayGoalScored = away;
            this.tip = tip;
            this.won = false;
        }


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
        public BetOffer BetOffer
        {
            get { return betOffer; }
            set { betOffer = value; }
        }

        //public int AwayGoalScored
        //{
        //    get
        //    {
        //        return awayGoalScored;
        //    }

        //    set
        //    {
        //        awayGoalScored = value;
        //    }
        //}

        //public int HomeGoalScored
        //{
        //    get
        //    {
        //        return homeGoalScored;
        //    }

        //    set
        //    {
        //        homeGoalScored = value;
        //    }
        //}

        public string Result
        {
            get { return result; }
            set { result = value; }
        }
    }
}
