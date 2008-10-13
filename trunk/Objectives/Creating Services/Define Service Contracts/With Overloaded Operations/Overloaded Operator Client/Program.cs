using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Overloaded_Operator_Client {

    class Program {

        static void Main(string[] args) {
            var proxy = new CalculatorService.CalculatorClient();
            
            // Unmodified client code
            //var n1 = proxy.AddInt(1, 2);
            //var n2 = proxy.AddDouble(1.0, 2.0);

            // Modified client code
            var n1 = proxy.Add(1, 2);
            var n2 = proxy.Add(1.0, 2.0);
            proxy.Close();

            Console.WriteLine("n1: {0}", n1);
            Console.WriteLine("n2: {0}", n2);
            Console.ReadKey(true);
        }
    }
}
