using System;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Xml;
using System.IO;

namespace WcfExamples.Messages
{
    public class CreateMessageExample
    {
        [DataContract(Name = "MyData", Namespace = "")]
        class PersonalData
        {
            [DataMember]
            public string Name;

            [DataMember]
            public int Age;
        }

        // Service
        [ServiceContract]
        interface IMyContract
        {
            [OperationContract(ReplyAction = "ResponseToGetDataRequest")]
            Message GetData();
        }

        [ServiceBehavior(IncludeExceptionDetailInFaults = true)]
        class MyService : IMyContract
        {
            [OperationBehavior]
            public Message GetData()
            {
                // Create object
                PersonalData data = new PersonalData() { Name = "Mark", Age = 35 };
                // Create message
                MessageVersion ver = OperationContext.Current.IncomingMessageVersion;
                Message msg = Message.CreateMessage(ver, "ResponseToGetDataRequest", data);
                Debug.WriteLine(msg.ToString());
                return msg;
            }
        }

        #region Host
        static string address;
        static ServiceHost host;

        public static void MyTestInitialize()
        {
            host = new ServiceHost(typeof(MyService));
            address = "net.pipe://localhost/" + Guid.NewGuid().ToString();
            host.AddServiceEndpoint(typeof(IMyContract), new NetNamedPipeBinding(), address);
            host.Open();
        }

        public static void MyTestCleanup()
        {
            if (host.State == CommunicationState.Opened)
                host.Close();
        }
        #endregion

        static public void Main(string[] args)
        {
            MyTestInitialize();
            IMyContract proxy = ChannelFactory<IMyContract>.CreateChannel(new NetNamedPipeBinding(), new EndpointAddress(address));
            using (proxy as IDisposable)
            {
                Message msg = proxy.GetData();
                XmlDictionaryReader xdr = msg.GetReaderAtBodyContents();
                string exp = "<MyData xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\"><Age>35</Age><Name>Mark</Name></MyData>";
                Debug.Assert(exp == xdr.ReadOuterXml());
                Console.WriteLine(msg.ToString());
            }
            MyTestCleanup();
        }
    }
}