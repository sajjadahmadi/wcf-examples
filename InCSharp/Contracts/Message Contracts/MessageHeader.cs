using System;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Channels;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WcfExamples.MessageContracts
{
    [TestClass]
    public class CustomHeaderExample
    {
        // Contracts
        [DataContract]
        class MyCustomType
        {
            [DataMember]
            public string MyMember;
        }

        [ServiceContract]
        interface IMyContract
        {
            [OperationContract]
            string GetHeaderString();
        }

        // Service
        [ServiceBehavior]
        class MyService : IMyContract
        {
            [OperationBehavior]
            public string GetHeaderString()
            {
                MyCustomType headerData =
                    OperationContext.Current.IncomingMessageHeaders.GetHeader<MyCustomType>("MyCustomType", "CodeRunner");
                return headerData.MyMember;
            }
        }

        // Client
        class MyContractClient : ClientBase<IMyContract>, IMyContract
        {
            public MyContractClient(string address)
                : base(new NetNamedPipeBinding(), new EndpointAddress(address))
            { }

            public string GetHeaderString()
            {
                MyCustomType headerData = new MyCustomType() { MyMember = "MyCustomType Header Data" };
                MessageHeader<MyCustomType> customHeader = new MessageHeader<MyCustomType>(headerData);

                using (OperationContextScope scope =
                    new OperationContextScope(InnerChannel))
                {
                    OperationContext.Current.OutgoingMessageHeaders.Add(
                        customHeader.GetUntypedHeader("MyCustomType", "CodeRunner"));
                    return Channel.GetHeaderString();
                }
            }
        }

        [TestMethod]
        public void CustomHeaderTest()
        {
            string address = "net.pipe://localhost/" + Guid.NewGuid().ToString();
            using (ServiceHost host = new ServiceHost(typeof(MyService)))
            {
                host.AddServiceEndpoint(typeof(IMyContract), new NetNamedPipeBinding(), address);
                host.Open();

                MyContractClient proxy = new MyContractClient(address);
                proxy.Open();
                Debug.Assert(proxy.GetHeaderString() == "MyCustomType Header Data");

                proxy.Close();
                host.Close();
            }
        }
    }
}