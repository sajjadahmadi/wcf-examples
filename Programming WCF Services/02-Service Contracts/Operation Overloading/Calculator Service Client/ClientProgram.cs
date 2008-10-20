using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.ServiceModel.Examples
{
    class ClientProgram
    {
        static void Main(string[] args)
        {
            double double1 = 10, double2 = 3;
            double doubleA;
            int int1 = 10, int2 = 3;
            int intA;

            Console.WriteLine("ServiceReferenceCalculator.CalculatorClient");
            ServiceReferenceCalculator.CalculatorClient c1 = new ServiceReferenceCalculator.CalculatorClient();
            doubleA = c1.AddDouble(double1, double2);
            Console.WriteLine("c1.AddDouble(double1, double2)=c1.AddDouble({0}, {1})={2}", double1, double2, doubleA);
            intA = c1.AddInt(int1, int2);
            Console.WriteLine("c1.AddInt(integer1, integer2)=c1.AddInt({0}, {1})={2}", int1, int2, intA);

            Console.WriteLine();

            Console.WriteLine("Examples.CalculatorClient");
            CalculatorClient c2 = new CalculatorClient();
            doubleA = c2.Add(double1, double2);
            Console.WriteLine("c2.Add(double1, double2)=c2.Add({0}, {1})={2}", double1, double2, doubleA);
            intA = c2.Add(int1, int2);
            Console.WriteLine("c2.Add(int1, int2)=c2.Add({0}, {1})={2}", int1, int2, intA);

            Console.ReadKey(true);
        }
    }
}
