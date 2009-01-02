using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace WcfExamples.Bindings
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public class CustomBindingExample
    {
        [ServiceContract(Name = "MyService")]
        interface IMyContract
        {
            [OperationContract]
            string MyMethod();
        }

        [ServiceBehavior(IncludeExceptionDetailInFaults = true)]
        class MyService : IMyContract
        {
            public string MyMethod()
            {
                return "Do something";
            }
        }

        class MyServiceClient : ClientBase<IMyContract>, IMyContract
        {
            public MyServiceClient(Binding binding, string address)
                : base(binding, new EndpointAddress(address))
            { }

            public string MyMethod()
            {
                return Channel.MyMethod();
            }
        }

        [TestMethod]
        public void CreateCustomBinding()
        {
            string address = "http://localhost:8888/MyService/" + Guid.NewGuid().ToString();
            using (ServiceHost host = new ServiceHost(typeof(MyService)))
            {
                BindingElementCollection bec = new BindingElementCollection();
                // The recommended order for BindingElements is: 
                // TransactionFlow, 
                // ReliableSession, 
                // Security, 
                // CompositeDuplex, 
                // OneWay, 
                // StreamSecurity, 
                // MessageEncoding, 
                // Transport.
                bec.Add(new TextMessageEncodingBindingElement());
                bec.Add(new HttpTransportBindingElement());

                CustomBinding binding = new CustomBinding(bec);

                host.AddServiceEndpoint(
                    typeof(IMyContract),
                    binding,
                    address);
                host.Open();

                using (MyServiceClient proxy = new MyServiceClient(binding, address))
                {
                    var response = proxy.MyMethod();
                    Assert.AreEqual("Do something", response);
                }
            }
        }
    }
}
