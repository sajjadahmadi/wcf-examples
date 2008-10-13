using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;


namespace DemoApplication
{
    class Program
    {
        static void Main(string[] args)
        {
            PerCallServiceReference.CounterServiceClient perCall = new PerCallServiceReference.CounterServiceClient();
            Console.WriteLine("PerCall Counter Service.");
            Console.WriteLine("Call 1: {0}", perCall.IncrementAndReturnCount());
            Console.WriteLine("Call 2: {0}", perCall.IncrementAndReturnCount());

            PerSessionServiceReference.CounterServiceClient perSession = new PerSessionServiceReference.CounterServiceClient();
            Console.WriteLine("PerSession Counter Service.");
            Console.WriteLine("Call 1: {0}", perSession.IncrementAndReturnCount());
            Console.WriteLine("Call 2: {0}", perSession.IncrementAndReturnCount());

            SingletonServiceReference.CounterServiceClient singleton = new SingletonServiceReference.CounterServiceClient();
            Console.WriteLine("Singleton Counter Service.");
            Console.WriteLine("Call 1: {0}", singleton.IncrementAndReturnCount());
            Console.WriteLine("Call 2: {0}", singleton.IncrementAndReturnCount());
                        
            Console.ReadKey();
        }
    }
}
