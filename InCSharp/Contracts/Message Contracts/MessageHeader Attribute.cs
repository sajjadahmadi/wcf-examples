using System;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.ServiceModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.ServiceModel.Description;

namespace WcfExamples.MessageContracts
{
    [TestClass]
    public class MessageHeaderAttributeExample
    {
        #region Contracts
        [DataContract]
        class ContactInfo
        {
            [DataMember]
            public string PhoneNumber;

            [DataMember]
            public string EmailAddress;
        }

        [MessageContract]
        class ContactInfoRequestMessage
        {
            [MessageHeader]
            public string LicenceKey;
        }

        [MessageContract]
        class ContactInfoResponseMessage
        {
            [MessageBodyMember]
            public ContactInfo ContactInfo;
        }

        [ServiceContract]
        interface ISomeService
        {
            [OperationContract]
            [FaultContract(typeof(string))]
            ContactInfoResponseMessage GetContactInfo(
                ContactInfoRequestMessage reqMsg);
        }
        #endregion

        #region Service
        class SomeService : ISomeService
        {
            public ContactInfoResponseMessage GetContactInfo(ContactInfoRequestMessage reqMsg)
            {
                if (reqMsg.LicenceKey != "some valid key")
                {
                    throw new FaultException<string>("Detail: Invalid license key: " + reqMsg.LicenceKey, "Reason: Invalid license key.");
                }

                ContactInfoResponseMessage respMsg =
                    new ContactInfoResponseMessage();
                respMsg.ContactInfo = new ContactInfo();
                respMsg.ContactInfo.EmailAddress = "some@email.com";
                respMsg.ContactInfo.PhoneNumber = "555-555-5555";

                return respMsg;
            }
        }
        #endregion

        #region Host
        ServiceHost host;
        string address;

        [TestInitialize()]
        public void MyTestInitialize()
        {
            host = new ServiceHost(typeof(SomeService));
            address = "net.pipe://localhost/" + Guid.NewGuid().ToString();
            host.AddServiceEndpoint(typeof(ISomeService), new NetNamedPipeBinding(), address);
            host.Description.Behaviors.Find<ServiceDebugBehavior>().IncludeExceptionDetailInFaults = true;
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
        [ExpectedException(typeof(FaultException<string>), "Reason: Invalid license key.")]
        public void RequestWithInvalidKey()
        {
            ISomeService proxy = ChannelFactory<ISomeService>.CreateChannel(new NetNamedPipeBinding(), new EndpointAddress(address));
            using (proxy as IDisposable)
            {
                ContactInfoRequestMessage reqMsg = new ContactInfoRequestMessage();
                reqMsg.LicenceKey = "some invalid key";

                ContactInfoResponseMessage respMsg;
                respMsg = proxy.GetContactInfo(reqMsg);
            }
        }

        [TestMethod]
        public void RequestWithValidKey()
        {
            ISomeService proxy = ChannelFactory<ISomeService>.CreateChannel(new NetNamedPipeBinding(), new EndpointAddress(address));
            using (proxy as IDisposable)
            {
                ContactInfoRequestMessage reqMsg = new ContactInfoRequestMessage();
                reqMsg.LicenceKey = "some valid key";

                ContactInfoResponseMessage respMsg;
                respMsg = proxy.GetContactInfo(reqMsg);

                Assert.AreEqual("555-555-5555", respMsg.ContactInfo.PhoneNumber);
                Assert.AreEqual("some@email.com", respMsg.ContactInfo.EmailAddress);
            }
        }
    }
}
