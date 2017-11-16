using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationPlatform
{
    [ServiceContract]
    public interface IBetService
    {
        [OperationContract]
        bool Login(string username, string password);

        [OperationContract]
        bool AddUser(User user);

        [OperationContract]
        bool DeleteUser(User user);

        [OperationContract]
        bool EditUser(User user);


        [OperationContract]
        bool SendTicket(Ticket ticket, string username);    
    }
}
