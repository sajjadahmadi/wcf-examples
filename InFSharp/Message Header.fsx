#light
#r "System.ServiceModel"
#r "System.Runtime.Serialization"
#load "InProcHost.fsx"
open System
open System.Runtime.Serialization
open System.ServiceModel
open Mcts_70_503


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


let host = new InProcHost<MyService>()
host.IncludeExceptionDetailInFaults <- true
host.AddEndpoint<IMyContract>()
host.Open()

let factory = new ChannelFactory<IMyContract>(new NetNamedPipeBinding())
let proxy = factory.CreateChannel(new EndpointAddress("net.pipe://localhost"))
let headerData = { Member1 = "value" }
let header = new MessageHeader<MyCustomType>(headerData)

let scope = new OperationContextScope(proxy :?> IContextChannel)
OperationContext.Current.OutgoingMessageHeaders.Add(
    header.GetUntypedHeader("MyCustomType", "ServiceModelEx"))
proxy.MyMethod()

