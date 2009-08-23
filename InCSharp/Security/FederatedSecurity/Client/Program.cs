using System;
using Client.SecureService;

namespace Client
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            using (var proxy = new SecureServiceContractClient())
            {
                var s = proxy.SendMessage("Hello from Client.");
                Console.WriteLine(s);
            }
        }
    }
}