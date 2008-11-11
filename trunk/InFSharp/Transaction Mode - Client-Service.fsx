#light
#r "System.ServiceModel"
#r "System.Runtime.Serialization"
#r "System.Transactions"
#load "InProcHost.fsx"
open System
open System.ServiceModel
open System.Transactions
open System.Diagnostics
open Mcts_70_503


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
    [<TransactionFlow(TransactionFlowOption.Allowed)>]
    abstract MyMethod : unit -> unit
        

type MyService() =
    interface IMyContract with
        [<OperationBehavior(TransactionScopeRequired = true)>]
        member this.MyMethod() =
            use scope = new TransactionScope()
            printTrans()
            scope.Complete()

// The Client/Service mode ensures the service uses the client transaction if
//   if possible or a service-side transaction when the client does not have
//   a transaction

let host = new InProcHost<MyService>()
// The Client/Service mode requires the use of a transaction-aware binding with
//   transaction flow enabled
let binding = new NetNamedPipeBinding(TransactionFlow = true)
host.AddEndpoint<IMyContract>(binding)
host.IncludeExceptionDetailInFaults <- true
host.Open()
let proxy = host.CreateProxy<IMyContract>()

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

host.CloseProxy(proxy)
host.Close()
