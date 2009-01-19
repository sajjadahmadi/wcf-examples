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
    public class CreateMessageBodyWriterExample
    {
        // Note:  Does not work the same with NetNamedPipeBinding (binary encoding)
        class TestDataWriter : BodyWriter
        {
            string _data;

            public TestDataWriter(string data)
                : base(false)
            {
                _data = data;
            }

            protected override void OnWriteBodyContents(XmlDictionaryWriter writer)
            {
                writer.WriteRaw(_data);
            }
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
                // Create body
                TestDataWriter body = new TestDataWriter("<test>data</test>");

                // Create messatge
                MessageVersion ver = OperationContext.Current.IncomingMessageVersion;
                Message msg = Message.CreateMessage(ver, "ResponseToGetDataRequest", body);

                Debug.WriteLine(msg.ToString());
                return msg;
            }
        }

        #region Host
        ServiceHost host;
        string address = "http://localhost/" + Guid.NewGuid().ToString();

        [TestInitialize()]
        public void MyTestInitialize()
        {
            host = new ServiceHost(typeof(MyService));
            host.AddServiceEndpoint(typeof(IMyContract), new BasicHttpBinding(), address);
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
        public void CreateMessageBodyWriter()
        {
            IMyContract proxy = ChannelFactory<IMyContract>.CreateChannel(new BasicHttpBinding(), new EndpointAddress(address));
            using (proxy as IDisposable)
            {
                Message msg = proxy.GetData();
                Debug.WriteLine(msg.ToString());
                XmlDictionaryReader xdr = msg.GetReaderAtBodyContents();
                string exp = "<test>data</test>";
                string act = xdr.ReadOuterXml();
                Assert.AreEqual(exp, act);
            }
        }
    }
}