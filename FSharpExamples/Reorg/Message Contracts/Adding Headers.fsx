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
    member this.PhoneNumber = pn
    member this.PhoneNumber with set v = pn <- v
    
    [<DataMember>]
    member this.EmailAddress = em
    member this.EmailAddress with set v = em <- v


[<MessageContract(IsWrapped = false)>]
type ContactInfoRequestMessage() =
    let mutable key = ""
    
    [<MessageHeader>]
    member this.LicenseKey = key
    member this.LicenseKey with set v = key <- v


[<MessageContract(IsWrapped = false)>]
type ContactInfoResponseMessage() =
    let mutable ci = ContactInfo()
    
    [<MessageBodyMember>]
    member this.ProviderContactInfo = ci
    member this.ProviderContactInfo with set v = ci <- v


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
                

let host = new ServiceHost(typeof<SomeService>, new Uri("net.tcp://localhost:8000"))
host.Open()

// First attempt calls service with invalid license key
let req1 = ContactInfoRequestMessage(LicenseKey = "ASdsds")
let proxy1 = ChannelFactory<ISomeService>.CreateChannel(host.Description.Endpoints.[0].Binding, host.Description.Endpoints.[0].Address)
try
    proxy1.GetProviderContactInfo(req1) |> ignore
with :? FaultException<string> as ex ->
    printfn "First call failed: %s\n" ex.Detail
    (proxy1 :?> ICommunicationObject).Abort()

// Second attempt calls service with correct key
let req2 = ContactInfoRequestMessage(LicenseKey = "abc-1234-alpha")
let proxy2 = ChannelFactory<ISomeService>.CreateChannel(host.Description.Endpoints.[0].Binding, host.Description.Endpoints.[0].Address)
let resp = proxy2.GetProviderContactInfo(req2)
printfn "Second call succeeded: Email: %s; Phone: %s" resp.ProviderContactInfo.EmailAddress resp.ProviderContactInfo.PhoneNumber
(proxy2 :?> ICommunicationObject).Close()
host.Close()
