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
        bool SendPort(byte[] username, byte[] port, byte[] address, byte[] Printport);

        [OperationContract]
        bool BetLogin(byte[] username, byte[] password, byte[] port);

        [OperationContract]
        bool AddUser(byte[] user);

        [OperationContract]
        bool DeleteUser(byte[] username);

        [OperationContract]
        bool EditUser(byte[] user);

        [OperationContract]
        bool SendTicket(byte[] ticket, byte[] username, byte[] port);

        [OperationContract]
        bool Deposit(byte[] acc, byte[] username);

        [OperationContract]
        bool GetServiceIP(byte[] AddressStringBytes);
        [OperationContract]
        bool IntrusionPrevention(byte[] user);
    }
}
