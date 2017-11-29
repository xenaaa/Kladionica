using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Contracts
{
    [ServiceContract]
    public interface IBankServiceIntegration 
    {
        [OperationContract]
        bool CheckIfAlive(int port);

        [OperationContract]
        bool BankLogin(byte[] username, byte[] password, byte[] port, byte[] address);

        [OperationContract]
        bool Deposit(byte[] acc, byte[] username, byte[] port);

        [OperationContract]
        bool CreateAccount(byte[] user, byte[] port);

        [OperationContract]
        List<Dictionary<string, int>> Report();
    }
}
