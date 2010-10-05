using System;

namespace Discovery.Service
{
    internal class DiscoverableService : IDiscoverableService
    {
        #region IDiscoverableService Members

        public void ServiceOperation()
        {
            Console.WriteLine("Call");
        }

        #endregion
    }
}