using System;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Channels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Xml;
using System.IO;

namespace WcfExamples.MessageContracts
{
    [TestClass]
    public class CreateMessageObjectExample
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
        ServiceHost host;
        string address;

        [TestInitialize()]
        public void MyTestInitialize()
        {
            host = new ServiceHost(typeof(MyService));
            address = "net.pipe://localhost/" + Guid.NewGuid().ToString();
            host.AddServiceEndpoint(typeof(IMyContract), new NetNamedPipeBinding(), address);
            host.Open();
        }

        [TestCleanup()]
        public void MyTestCleanup()
        {
            if (host.State == CommunicationState.Opened)
                host.Close();
        }
        #endregion

        [TestMethod]
        public void CreateMessageFromObject()
        {
            IMyContract proxy = ChannelFactory<IMyContract>.CreateChannel(new NetNamedPipeBinding(), new EndpointAddress(address));
            using (proxy as IDisposable)
            {
                Message msg = proxy.GetData();
                Debug.WriteLine(msg.ToString());
                XmlDictionaryReader xdr = msg.GetReaderAtBodyContents();
                string exp = "<MyData xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\"><Age>35</Age><Name>Mark</Name></MyData>";
                Assert.AreEqual(exp, xdr.ReadOuterXml());
            }
        }
    }
}