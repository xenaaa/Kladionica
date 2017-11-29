﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Contracts
{
    [ServiceContract]
    public interface IBetServiceIntegration :  IClientBet , IBetServiceOnBank
    {
        //[OperationContract]
        //bool CheckIfAlive(int port);

        //[OperationContract]
        //bool SendPort(byte[] username, byte[] port, byte[] address, byte[] Printport);

        //[OperationContract]
        //bool BetLogin(byte[] username, byte[] password, byte[] port);

        //[OperationContract]
        //bool AddUser(byte[] user, byte[] port);

        //[OperationContract]
        //bool DeleteUser(byte[] username, byte[] port);

        //[OperationContract]
        //bool EditUser(byte[] user, byte[] port);

        //[OperationContract]
        //bool SendTicket(byte[] ticket, byte[] username, byte[] port);

        //[OperationContract]
        //bool Deposit(byte[] acc, byte[] username, byte[] port);

        //[OperationContract]
        //bool GetServiceIP(byte[] AddressStringBytes);

        //[OperationContract]
        //List<Dictionary<string, int>> Report();
    }
}
