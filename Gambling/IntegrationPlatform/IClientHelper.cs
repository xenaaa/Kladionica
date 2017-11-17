using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationPlatform
{
    [ServiceContract]
    public interface IClientHelper
    {
        [OperationContract]
        bool CheckIfAlive();

        [OperationContract]
        bool SendGameResults(List<string> results);

        [OperationContract]
        bool SendOffers(Dictionary<int,BetOffer> offers);

        [OperationContract]
        bool SendTicketResults(Ticket tiket,bool prosao);
    }
}
