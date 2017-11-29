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
        bool CheckIfAlive(int port);

        [OperationContract]
        bool BankLogin(byte[] username, byte[] password, byte[] address);

        [OperationContract]
        bool Deposit(byte[] acc, byte[] username);

        [OperationContract]
        bool CreateAccount(byte[] user);

    }
}
