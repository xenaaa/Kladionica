using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Contracts
{
    [ServiceContract]
    public interface IBetServiceOnBank
    {
        [OperationContract]
        bool Deposit(byte[] acc, byte[] username);

        [OperationContract]
        bool GetServiceIP(byte[] AddressStringBytes);
    }
}
