﻿using System;
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
        bool SendPort(int port);
        
        [OperationContract]
        bool BetLogin(string username, string password,int port);

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
