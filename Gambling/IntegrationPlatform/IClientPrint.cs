using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Contracts
{
    [ServiceContract]
    public interface IClientPrint
    {
        [OperationContract]
        bool CheckIfAlive();

        [OperationContract]
        bool SendGameResults(byte[] results);

        [OperationContract]
        bool SendOffers(byte[] offers);       

        [OperationContract]
        bool CloseProxy();
    }
}
