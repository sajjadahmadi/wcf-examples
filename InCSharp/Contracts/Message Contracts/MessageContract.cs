using System;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.ServiceModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WcfExamples.MessageContracts
{
    [TestClass]
    public class MessageContractExample
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

        [MessageContract(IsWrapped = false)]
        class ContactInfoRequestMessage
        {
            [MessageHeader]
            public string LicenceKey;
        }

        [MessageContract(IsWrapped = false)]
        class ContactInfoResponseMessage
        {
            [MessageBodyMember]
            public ContactInfo ProviderContactInfo;
        }
        #endregion

        #region Service
        [ServiceContract]
        interface ISomeService
        {
            [OperationContract]
            [FaultContract(typeof(string))]
            ContactInfoResponseMessage GetProviderContactInfo(
                ContactInfoRequestMessage reqMsg);
        }

        class SomeService : ISomeService
        {
            public ContactInfoResponseMessage GetProviderContactInfo(ContactInfoRequestMessage reqMsg)
            {
                if (reqMsg.LicenceKey != "some valid key")
                {
                    throw new FaultException<string>("Invalid license key.");
                }

                ContactInfoResponseMessage respMsg =
                    new ContactInfoResponseMessage();
                respMsg.ProviderContactInfo = new ContactInfo();
                respMsg.ProviderContactInfo.EmailAddress = "some@email.com";
                respMsg.ProviderContactInfo.PhoneNumber = "555-555-5555";

                return respMsg;
            }
        }
        #endregion

        #region Client
        class SomeServiceClient : ClientBase<ISomeService>, ISomeService
        {
            public SomeServiceClient(string address)
                : base(new NetNamedPipeBinding(), new EndpointAddress(address)) { }

            public ContactInfoResponseMessage GetProviderContactInfo(ContactInfoRequestMessage reqMsg)
            {
                return Channel.GetProviderContactInfo(reqMsg);
            }

        }
        #endregion

        [TestMethod]
        public void MessageContractTest()
        {
            using (ServiceHost host = new ServiceHost(typeof(SomeService)))
            {
                string address = "net.pipe://localhost/" + Guid.NewGuid().ToString();
                host.AddServiceEndpoint(typeof(ISomeService), new NetNamedPipeBinding(), address);
                host.Open();

                using (SomeServiceClient proxy = new SomeServiceClient(address))
                {
                    proxy.Open();

                    ContactInfoRequestMessage reqMsg = new ContactInfoRequestMessage();
                    reqMsg.LicenceKey = "some valid key";

                    ContactInfoResponseMessage respMsg;
                    respMsg = proxy.GetProviderContactInfo(reqMsg);

                    Assert.AreEqual("555-555-5555", respMsg.ProviderContactInfo.PhoneNumber);
                    Assert.AreEqual("some@email.com", respMsg.ProviderContactInfo.EmailAddress);
                }
            }
        }
    }
}
