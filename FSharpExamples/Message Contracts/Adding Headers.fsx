#light
#r "System.ServiceModel"
#r "System.Runtime.Serialization"
open System
open System.ServiceModel
open System.ServiceModel.Description
open System.Runtime.Serialization


[<DataContract>]
type ContactInfo() =
    let mutable pn = ""
    let mutable em = ""
    
    [<DataMember>]
    member this.PhoneNumber
        with get() = pn
        and set v = pn <- v
    
    [<DataMember>]
    member this.EmailAddress
        with get() = em
        and set v = em <- v


[<MessageContract(IsWrapped = false)>]
type ContactInfoRequestMessage() =
    let mutable key = ""
    
    [<MessageHeader>]
    member this.LicenseKey
        with get() = key
        and set v = key <- v


[<MessageContract(IsWrapped = false)>]
type ContactInfoResponseMessage() =
    let mutable ci = ContactInfo()
    
    [<MessageBodyMember>]
    member this.ProviderContactInfo
        with get() = ci
        and set v = ci <- v


[<ServiceContract>]
type ISomeService =
    [<OperationContract>]
    [<FaultContract(typeof<string>)>]
    abstract GetProviderContactInfo : ContactInfoRequestMessage -> ContactInfoResponseMessage


type SomeService() =
    interface ISomeService with
        member this.GetProviderContactInfo(reqMsg) =
            if reqMsg.LicenseKey <> "abc-1234-alpha" then
                let msg = "Invalid license key."
                raise (new FaultException<string>(msg))
            
            let info = ContactInfo(EmailAddress = "sam@fabrikam.com", 
                                   PhoneNumber = "123-456-7890")
            let respMsg = ContactInfoResponseMessage(ProviderContactInfo = info)
            respMsg
                

let uri = new Uri("net.tcp://localhost:8000")
let binding = new NetTcpBinding()
let host = new ServiceHost(typeof<SomeService>, [| uri |])
let debug = host.Description.Behaviors.Find<ServiceDebugBehavior>()
debug.IncludeExceptionDetailInFaults <- true
host.AddServiceEndpoint(typeof<ISomeService>, binding, "")
host.Open()

// First attempt calls service with invalid license key
let req1 = ContactInfoRequestMessage(LicenseKey = "ASdsds")
let proxy1 = ChannelFactory<ISomeService>.CreateChannel(binding, new EndpointAddress(string uri))
try
    let resp = proxy1.GetProviderContactInfo(req1)
    ()
with :? FaultException<string> as ex ->
    printfn "%s\n------------------" ex.Detail
    (proxy1 :?> ICommunicationObject).Abort()

// Second attempt calls service with correct key
let req2 = ContactInfoRequestMessage(LicenseKey = "abc-1234-alpha")
let proxy2 = ChannelFactory<ISomeService>.CreateChannel(binding, new EndpointAddress(string uri))
let resp = proxy2.GetProviderContactInfo(req2)
printfn "Email: %s; Phone: %s" resp.ProviderContactInfo.EmailAddress resp.ProviderContactInfo.PhoneNumber
(proxy2 :?> ICommunicationObject).Close()
host.Close()
