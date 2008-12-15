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


[<ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)>]
type MyService() =
    interface IMyContract with
        [<OperationBehavior(TransactionScopeRequired = true)>]
        member this.MyMethod() =
            let hdrs = OperationContext.Current.IncomingMessageHeaders
            printfn "\nIncomingHeaders\n-----------------"
            for h in hdrs do
                printfn "%s: %s\n" h.Name (string h)


type MyServiceClient() =
    inherit ClientBase<IMyContract>(new NetNamedPipeBinding(), new EndpointAddress("net.pipe://localhost"))
    
    let c = base.ChannelFactory.CreateChannel()
    
    member this.MyMethod() =
        let hdrs = OperationContext.Current.OutgoingMessageHeaders
        printfn "OutgoingHeaders\n------------------"
        for h in hdrs do
            printfn "%s: %s\n" h.Name (string h)
        c.MyMethod()


let binding = new NetNamedPipeBinding()
binding.TransactionFlow <- true
let host = new InProcHost<MyService>()
host.IncludeExceptionDetailInFaults <- true
host.AddEndpoint<IMyContract>(binding)
host.Open()

let client = new MyServiceClient()
let headerData = { Member1 = "value" }
let header = new MessageHeader<MyCustomType>(headerData)
using (new OperationContextScope(client.InnerChannel)) (fun scope ->
    OperationContext.Current.OutgoingMessageHeaders.Add(
        header.GetUntypedHeader("MyCustomType", "ServiceModelEx"))
    client.MyMethod()
    Threading.Thread.Sleep(100))
client.Close()
