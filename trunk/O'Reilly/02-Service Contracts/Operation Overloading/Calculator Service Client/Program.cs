using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.ServiceModel.Examples
{
    class Program
    {
        static void Main(string[] args)
        {
            CalculatorClient c = new CalculatorClient();
            double double1 = 3, double2 = 6;
            double answer = c.Add(double1, double2);
            Console.WriteLine("c.Add(double1, double2)=c.Add({0}, {1})={2}", double1,double2,answer);
            Console.ReadKey(true);
        }
    }
}
