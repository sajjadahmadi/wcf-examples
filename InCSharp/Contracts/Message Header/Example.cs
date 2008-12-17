using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.Runtime.Serialization;
using System.ServiceModel.Channels;
using System.Diagnostics;

namespace CodeRunner
{
    class Example
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
            void MyMethod();
        }

        // Service
        [ServiceBehavior(IncludeExceptionDetailInFaults = true)]
        class MyService : IMyContract
        {
            [OperationBehavior(TransactionScopeRequired = true)]
            public void MyMethod()
            {
                MyCustomType headerData =
                    OperationContext.Current.IncomingMessageHeaders.GetHeader<MyCustomType>("MyCustomType", "CodeRunner");
                Debug.Assert(headerData.MyMember == "MyCustomType Header Data");
                return;
            }
        }

        // Client
        class MyContractClient : ClientBase<IMyContract>, IMyContract
        {
            public MyContractClient(string address)
                : base(new NetNamedPipeBinding(), new EndpointAddress(address))
            { }

            public void MyMethod()
            {
                MyCustomType headerData = new MyCustomType() { MyMember = "MyCustomType Header Data" };
                MessageHeader<MyCustomType> customHeader = new MessageHeader<MyCustomType>(headerData);

                using (OperationContextScope scope =
                    new OperationContextScope(InnerChannel))
                {
                    OperationContext.Current.OutgoingMessageHeaders.Add(
                        customHeader.GetUntypedHeader("MyCustomType", "CodeRunner"));
                    Channel.MyMethod();
                }
            }
        }

        static void Main(string[] args)
        {
            string address = "net.pipe://localhost/" + Guid.NewGuid().ToString();
            using (ServiceHost host = new ServiceHost(typeof(MyService)))
            {
                host.AddServiceEndpoint(typeof(IMyContract), new NetNamedPipeBinding(), address);
                host.Open();

                MyContractClient proxy = new MyContractClient(address);
                proxy.Open();
                proxy.MyMethod();

                proxy.Close();
                host.Close();
            }
        }
    }
}