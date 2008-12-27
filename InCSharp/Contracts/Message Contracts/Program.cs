using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Diagnostics;

namespace Message_Contracts
{
    #region Contracts
    [DataContract]
    public class ContactInfo
    {
        [DataMember]
        public string PhoneNumber;

        [DataMember]
        public string EmailAddress;
    }

    [MessageContract(IsWrapped = false)]
    public class ContactInfoRequestMessage
    {
        [MessageHeader]
        public string LicenceKey;
    }

    [MessageContract(IsWrapped = false)]
    public class ContactInfoResponseMessage
    {
        [MessageBodyMember]
        public ContactInfo ProviderContactInfo;
    }
    #endregion

    #region Service
    [ServiceContract]
    public interface ISomeService
    {
        [OperationContract]
        [FaultContract(typeof(string))]
        ContactInfoResponseMessage GetProviderContactInfo(
            ContactInfoRequestMessage reqMsg);
    }

    public class SomeService : ISomeService
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

    class Program
    {
        static void Main(string[] args)
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

                    Debug.Assert(respMsg.ProviderContactInfo.PhoneNumber == "555-555-5555");
                    Debug.Assert(respMsg.ProviderContactInfo.EmailAddress == "some@email.com");
                }
            }
        }
    }
}
