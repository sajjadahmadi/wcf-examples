//css_ref System.Runtime.Serialization.dll;
using System;
using System.Diagnostics;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Xml;

namespace WcfExamples.MessageContracts
{
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
        static ServiceHost host;
        static string address = "http://localhost/" + Guid.NewGuid().ToString();

        public static void MyTestInitialize()
        {
            host = new ServiceHost(typeof(MyService));
            host.AddServiceEndpoint(typeof(IMyContract), new BasicHttpBinding(), address);
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
            IMyContract proxy = ChannelFactory<IMyContract>.CreateChannel(new BasicHttpBinding(), new EndpointAddress(address));
            using (proxy as IDisposable)
            {
                Message msg = proxy.GetData();
                Console.WriteLine(msg.ToString());
                Console.WriteLine();

                XmlDictionaryReader xdr = msg.GetReaderAtBodyContents();
                string exp = "<test>data</test>";
                string act = xdr.ReadOuterXml();
                Debug.Assert(exp == act);
                Console.WriteLine(act);
            }
            MyTestCleanup();
        }
    }
}