using System;
using System.ServiceModel;

namespace Greeting.Service
{
    [ServiceBehavior(IncludeExceptionDetailInFaults = true)]
    internal class GreetingService : IHelloService, IGoodbyeService
    {
        #region IGoodbyeService Members

        public string SayGoodbye()
        {
            Console.WriteLine("Goodbye Serevice call...");
            return "Goodbye";
        }

        #endregion

        #region IHelloService Members

        public string SayHello()
        {
            Console.WriteLine("Hello Service call...");
            return "Hello";
        }

        #endregion
    }
}