using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ServiceProxy
{
    class Program
    {
        static void Main(string[] args)
        {
            MyServiceReference.MyContractClient proxy = new MyServiceReference.MyContractClient();
            Console.WriteLine( proxy.MyOperation());
            Console.ReadKey(true);
            proxy.Close();
        }
    }
}
