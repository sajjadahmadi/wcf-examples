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
            ServiceHost host = new ServiceHost(typeof(Calculator), new Uri("http://localhost:8000"));
            WSHttpBinding binding = new WSHttpBinding();
            host.AddServiceEndpoint("System.ServiceModel.Examples.ICalculator", binding, "Calculator");
            host.Open();
            Console.ReadKey(true);
            host.Close();
        }
    }
}
