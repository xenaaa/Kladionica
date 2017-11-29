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
        bool CheckIfAlive(byte[] portBytes, byte[] addressBytes, byte[] isItPrintClientBytes);

        [OperationContract]
        bool SendGameResults(byte[] results, byte[] port, byte[] address);

        [OperationContract]
        bool SendOffers(byte[] offers, byte[] port, byte[] addressBytes, byte[] isItPrintClientBytes);       

        [OperationContract]
        bool CloseProxy();
    }
}
