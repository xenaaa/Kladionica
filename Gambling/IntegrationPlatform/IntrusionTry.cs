using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationPlatform
{
    public class IntrusionTry
    {
        private int attempt;

        public int Attempt
        {
            get { return attempt; }
            set { attempt = value; }
        }

        private DateTime lastTry;

        public DateTime LastTry
        {
            get { return lastTry; }
            set { lastTry = value; }
        }
        public IntrusionTry(int attempt,DateTime lastTry)
        {
            Attempt = attempt;
            LastTry = lastTry;
        }
    }

}
