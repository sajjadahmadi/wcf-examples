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
                printfn "%s: %O\n" h.Name h


let host = new ServiceHost(typeof<MyService>, new Uri("http://localhost"))
host.Open()

let headerData = { Member1 = "value" }
let messageHeader = new MessageHeader<MyCustomType>(headerData)

let proxy = ChannelFactory<IMyContract>.CreateChannel(host.Description.Endpoints.[0].Binding, host.Description.Endpoints.[0].Address)

let scope = new OperationContextScope(proxy :?> IContextChannel)
let header = messageHeader.GetUntypedHeader("MyCustomType", "ServiceModelEx")
OperationContext.Current.OutgoingMessageHeaders.Add(header)

proxy.MyMethod()

host.Close()