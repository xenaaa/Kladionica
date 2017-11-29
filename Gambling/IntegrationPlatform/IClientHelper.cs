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
        bool CheckIfAlive();


        [OperationContract]
        bool SendOffers(byte[] offers);

        [OperationContract]
        bool SendTicketResults(byte[] ticket, byte[] isPassed);

        [OperationContract]
       bool CloseProxy();

    }
}
