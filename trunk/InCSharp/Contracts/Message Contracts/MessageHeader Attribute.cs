using System;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Description;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WcfExamples.MessageContracts
{
    [TestClass]
    public class MessageHeaderAttributeExample
    {
        #region Contracts

        #region Nested type: ContactInfo

        [DataContract]
        private class ContactInfo
        {
            [DataMember] public string EmailAddress;
            [DataMember] public string PhoneNumber;
        }

        #endregion

        #region Nested type: ContactInfoRequestMessage

        [MessageContract]
        private class ContactInfoRequestMessage
        {
            [MessageHeader] public string LicenceKey;
        }

        #endregion

        #region Nested type: ContactInfoResponseMessage

        [MessageContract]
        private class ContactInfoResponseMessage
        {
            [MessageBodyMember] public ContactInfo ContactInfo;
        }

        #endregion

        #region Nested type: ISomeService

        [ServiceContract]
        private interface ISomeService
        {
            [OperationContract]
            [FaultContract(typeof(string))]
            ContactInfoResponseMessage GetContactInfo(
                    ContactInfoRequestMessage reqMsg);
        }

        #endregion

        #endregion

        #region Service

        private class SomeService : ISomeService
        {
            #region ISomeService Members

            public ContactInfoResponseMessage GetContactInfo(ContactInfoRequestMessage reqMsg)
            {
                if (reqMsg.LicenceKey != "some valid key")
                    throw new FaultException<string>("Detail: Invalid license key: " + reqMsg.LicenceKey,
                                                     "Reason: Invalid license key.");

                var respMsg =
                        new ContactInfoResponseMessage();
                respMsg.ContactInfo = new ContactInfo();
                respMsg.ContactInfo.EmailAddress = "some@email.com";
                respMsg.ContactInfo.PhoneNumber = "555-555-5555";

                return respMsg;
            }

            #endregion
        }

        #endregion

        #region Host

        private string address;
        private ServiceHost host;

        [TestInitialize]
        public void MyTestInitialize()
        {
            host = new ServiceHost(typeof(SomeService));
            address = "net.pipe://localhost/" + Guid.NewGuid();
            host.AddServiceEndpoint(typeof(ISomeService), new NetNamedPipeBinding(), address);
            host.Description.Behaviors.Find<ServiceDebugBehavior>().IncludeExceptionDetailInFaults = true;
            host.Open();
        }

        [TestCleanup]
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
            var proxy = ChannelFactory<ISomeService>.CreateChannel(new NetNamedPipeBinding(),
                                                                   new EndpointAddress(address));
            using (proxy as IDisposable)
            {
                var reqMsg = new ContactInfoRequestMessage();
                reqMsg.LicenceKey = "some invalid key";

                ContactInfoResponseMessage respMsg;
                respMsg = proxy.GetContactInfo(reqMsg);
            }
        }

        [TestMethod]
        public void RequestWithValidKey()
        {
            var proxy = ChannelFactory<ISomeService>.CreateChannel(new NetNamedPipeBinding(),
                                                                   new EndpointAddress(address));
            using (proxy as IDisposable)
            {
                var reqMsg = new ContactInfoRequestMessage { LicenceKey = "some valid key" };

                var respMsg = proxy.GetContactInfo(reqMsg);

                Assert.AreEqual("555-555-5555", respMsg.ContactInfo.PhoneNumber);
                Assert.AreEqual("some@email.com", respMsg.ContactInfo.EmailAddress);
            }
        }
    }
}