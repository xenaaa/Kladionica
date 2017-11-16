﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationPlatform
{
    [ServiceContract]
    public interface IClientHelper
    {

        [OperationContract]
        bool SendGameResults(List<string> results);

        [OperationContract]
        bool SendOffers(List<BetOffer> offers);

        [OperationContract]
        bool SendTicketResults();
    }
}
