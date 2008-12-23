#light
#r "System.ServiceModel"
#r "System.Runtime.Serialization"
#r "System.Transactions"
open System
open System.ServiceModel
open System.ServiceModel.Channels
open System.Transactions
open System.Diagnostics


let printTrans() =
    let ambTrans = Transaction.Current
    if ambTrans <> null then
        let info = ambTrans.TransactionInformation
        printfn "Dist  ID %A\nLocal ID %s" info.DistributedIdentifier info.LocalIdentifier
    else
        printfn "No Transaction"
            
            
[<ServiceContract>]
type IMyContract =
    [<OperationContract>]
    [<TransactionFlow(TransactionFlowOption.NotAllowed)>]
    abstract MyMethod : unit -> unit
        

type MyService() =
    interface IMyContract with
        [<OperationBehavior(TransactionScopeRequired = false)>]
        member this.MyMethod() =
            printTrans()

// The None transaction mode means the service never has a transaction
let uri = new Uri("net.tcp://localhost")
let host = new ServiceHost(typeof<MyService>, [| uri |])
let binding = new NetTcpBinding(TransactionFlow = false)
host.AddServiceEndpoint(typeof<IMyContract>, binding, "")
host.Open()

let proxy = ChannelFactory<IMyContract>.CreateChannel(binding, new EndpointAddress(string uri))

printfn "No Scope"
proxy.MyMethod()
printfn "---------------------"

let scope = new TransactionScope()

printfn "Local Scope"
printTrans()
printfn "---------------------"

printfn "Service Scope"
proxy.MyMethod()
printfn "---------------------"

scope.Complete()
scope.Dispose()

(proxy :?> ICommunicationObject).Close()
host.Close()
