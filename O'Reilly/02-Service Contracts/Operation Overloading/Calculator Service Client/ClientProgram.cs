using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel.Examples;

namespace System.ServiceModel.Examples
{
    class ClientProgram
    {
        static void Main(string[] args)
        {
            CalculatorServiceReference.CalculatorClient c = new CalculatorServiceReference.CalculatorClient();
            double double1 = 3, double2 = 6;
            double answer = c.Add(double1, double2);
            Console.WriteLine("c.Add(double1, double2)=c.Add({0}, {1})={2}", double1, double2, answer);

            CalculatorClient c2 = new CalculatorClient();
            double1 = 3;
            double2 = 6;
            answer = c2.Add(double1, double2);
            Console.WriteLine("c.Add(double1, double2)=c.Add({0}, {1})={2}", double1, double2, answer);

            Console.ReadKey(true);
        }
    }
}
