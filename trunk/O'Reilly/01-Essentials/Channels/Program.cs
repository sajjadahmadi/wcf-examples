using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Channels;

namespace Channels
{
    [ServiceContract]
    public interface IMyContract
    {

        [OperationContract]
        string MyOperation();
    }

    class Program
    {
        static void Main(string[] args)
        {
            ChannelFactory<IMyContract> factory;
            factory = new ChannelFactory<IMyContract>("MyEndpoint");
            IMyContract proxy = factory.CreateChannel();
            using (proxy as IDisposable)
            {
                string result = proxy.MyOperation();
                Console.WriteLine("MyOperation: {0}", result);
            }
            Console.ReadKey(true);
        }
    }
}
