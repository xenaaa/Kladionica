﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Contracts
{
    [ServiceContract]
    public interface IClientHelper
    {
        [OperationContract]
        bool CheckIfAlive(byte[] portBytes);

        [OperationContract]
        bool SendGameResults(byte[] results, byte[] port);

        [OperationContract]
        bool SendOffers(byte[] offers, byte[] port);

        [OperationContract]
        bool SendTicketResults(byte[] ticket, byte[] isPassed, byte[] port);
    }
}
