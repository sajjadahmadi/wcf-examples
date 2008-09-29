using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Overloaded_Operator_Client {

    class Program {

        static void Main(string[] args) {
            var client = new CalculatorService.CalculatorClient();
            
            // Unmodified client code
            //var n1 = client.AddInt(1, 2);
            //var n2 = client.AddDouble(1.0, 2.0);

            // Modified client code
            var n1 = client.Add(1, 2);
            var n2 = client.Add(1.0, 2.0);
            Console.WriteLine("n1: {0}", n1);
            Console.WriteLine("n2: {0}", n2);
            Console.ReadKey(true);
        }
    }
}
