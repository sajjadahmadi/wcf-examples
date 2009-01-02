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
    public class CustomeBindingExample
    {
        [ServiceContract(Name = "MyService")]
        interface IMyContract
        {
            [OperationContract]
            void MyMethod();
        }

        class MyService : IMyContract
        {
            public void MyMethod()
            {
                // Do something
            }
        }

        class MyServiceClient : ClientBase<IMyContract>, IMyContract
        {
            public MyServiceClient(Binding binding, string address)
                : base(binding, new EndpointAddress(address))
            { }

            public void MyMethod()
            {
                Channel.MyMethod();
            }
        }

        [TestMethod]
        public void TestMethod1()
        {
            string address = "http://localhost:8000/MyService" //+ Guid.NewGuid().ToString();
            using (ServiceHost host = new ServiceHost(typeof(MyService)))
            {
                SymmetricSecurityBindingElement ssbe = new SymmetricSecurityBindingElement();
                ssbe.LocalServiceSettings.InactivityTimeout = new TimeSpan(0, 10, 0);

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
                bec.Add(ssbe);
                bec.Add(new TextMessageEncodingBindingElement());
                bec.Add(new HttpTransportBindingElement());

                CustomBinding binding = new CustomBinding(bec);

                host.AddServiceEndpoint(
                    typeof(IMyContract),
                    binding,
                    address);

                MyServiceClient proxy = new MyServiceClient(binding, address);
                proxy.Open();
                proxy.MyMethod();

                proxy.Close();
            }
        }
    }
}
