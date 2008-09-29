using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hierarchy_Client {

    class Program {

        static void Main(string[] args) {
            var proxy1 = new CalculatorService.SimpleCalculatorClient();
            var n1 = proxy1.Add(1, 2);
            proxy1.Close();

            var proxy2 = new CalculatorService.ScientificCalculatorClient();
            var n2 = proxy2.Add(3, 4);
            var n3 = proxy2.Multiply(5, 6);
            proxy2.Close();

            Console.WriteLine("n1: {0}", n1);
            Console.WriteLine("n2: {0}", n2);
            Console.WriteLine("n3: {0}", n3);



            Console.ReadKey(true);
        }
    }
}
