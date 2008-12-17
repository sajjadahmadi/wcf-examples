using System;

namespace ServiceProxy
{
    class Program
    {
        // Start the Self Hosting example located at Repository\O'Reilly\01-Essentials\Hosting\Self Hosting
        static void Main(string[] args)
        {
            MyContractClient proxy = new MyContractClient();
            Console.WriteLine(proxy.MyOperation());
            Console.ReadKey(true);
            proxy.Close();
        }
    }
}
