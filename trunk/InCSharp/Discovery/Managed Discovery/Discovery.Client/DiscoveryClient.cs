using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;

namespace Discovery.Client
{
    class DiscoveryClient : ClientBase<IDiscoverableService>, IDiscoverableService
    {
        public void ServiceOperation()
        {
            Channel.ServiceOperation();
        }
    }
}
