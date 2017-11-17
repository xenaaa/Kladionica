using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationPlatform
{
    [ServiceContract]
    public interface IBankService
    {
        [OperationContract]
        bool CheckIfAlive();
        [OperationContract]
        bool Login(string username, string password,int port);

        [OperationContract]
        bool Deposit(Account acc);
    }
}
