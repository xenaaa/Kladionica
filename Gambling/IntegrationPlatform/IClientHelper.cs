using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Contracts
{
    [ServiceContract]
    public interface IClientHelper
    {
        [OperationContract]
        bool CheckIfAlive(int port);

        [OperationContract]
        bool SendGameResults(List<Game> results, int port);

        [OperationContract]
        bool SendOffers(Dictionary<int, BetOffer> offers, int port);

        [OperationContract]
        bool SendTicketResults(Ticket ticket, bool isPassed, int port);
    }
}
