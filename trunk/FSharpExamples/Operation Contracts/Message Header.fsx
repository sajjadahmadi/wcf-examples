#light
#r "System.ServiceModel"
#r "System.Runtime.Serialization"
open System
open System.Runtime.Serialization
open System.ServiceModel


[<DataContract>]
type MyCustomType =
    { [<DataMember>] mutable Member1 : string }
    

[<ServiceContract>]
type IMyContract =
    [<OperationContract>]
    abstract MyMethod : unit -> unit


type MyService() =
    interface IMyContract with
        member this.MyMethod() =
            let hdrs = OperationContext.Current.IncomingMessageHeaders
            printfn "\nIncomingHeaders\n-----------------"
            for h in hdrs do
                printfn "%s: %A\n" h.Name h


let uri = new Uri("net.tcp://localhost")
let binding = new NetTcpBinding()
let host = new ServiceHost(typeof<MyService>, [| uri |])
host.AddServiceEndpoint(typeof<IMyContract>, binding, "")
host.Open()

let proxy = ChannelFactory<IMyContract>.CreateChannel(binding, new EndpointAddress(string uri))
let headerData = { Member1 = "value" }
let header = new MessageHeader<MyCustomType>(headerData)

let scope = new OperationContextScope(proxy :?> IContextChannel)
OperationContext.Current.OutgoingMessageHeaders.Add(
    header.GetUntypedHeader("MyCustomType", "ServiceModelEx"))
proxy.MyMethod()

