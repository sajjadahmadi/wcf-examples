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
    [<TransactionFlow(TransactionFlowOption.Mandatory)>]
    abstract MyMethod : unit -> unit
        

type MyService() =
    interface IMyContract with
        [<OperationBehavior(TransactionScopeRequired = true)>]
        member this.MyMethod() =
            printTrans()

// The Client mode ensures the service only uses the client's transaction

let host = new InProcHost<MyService>()
// The Client mode requires the use of a transaction-aware binding with
//   transaction flow enabled
let binding = new NetNamedPipeBinding(TransactionFlow = true)
host.AddEndpoint<IMyContract>(binding)
host.Open()
let proxy = host.CreateProxy<IMyContract>()

printfn "No Scope"
printTrans()
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
