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
        bool CreateAccount(User user);

        [OperationContract]
        bool Deposit(Account acc,User user);
    }
}
