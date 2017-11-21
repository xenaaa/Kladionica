using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Contracts
{
    [ServiceContract]
    public interface IBetService
    {
        [OperationContract]
        bool CheckIfAlive();

        [OperationContract]
        bool SendPort(string username, int port);

        [OperationContract]
        bool BetLogin(string username, string password, int port);

        [OperationContract]
        bool AddUser(User user);

        [OperationContract]
        bool DeleteUser(string username);

        [OperationContract]
        bool EditUser(User user);


        [OperationContract]
        bool SendTicket(Ticket ticket, string username);
    }
}
