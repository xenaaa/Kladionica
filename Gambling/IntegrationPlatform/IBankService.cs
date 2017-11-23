using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Contracts
{
    [ServiceContract]
    public interface IBankService
    {
        [OperationContract]
        bool CheckIfAlive();
        [OperationContract]
        bool BankLogin(byte[] username, byte[] password, byte[] port,byte[] address);
        [OperationContract]
        bool Deposit(byte[] acc, byte[] username);
        [OperationContract]
        bool CreateAccount(byte[]  user);
        [OperationContract]
        bool IntrusionPrevention(byte[] user);


    }
}
