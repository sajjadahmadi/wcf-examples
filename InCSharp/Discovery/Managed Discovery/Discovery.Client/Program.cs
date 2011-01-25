using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;

namespace Discovery.Client
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var client = new DiscoveryClient())
            {
                client.ServiceOperation();
            }
        }
    }
}
