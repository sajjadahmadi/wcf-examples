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
    [<TransactionFlow(TransactionFlowOption.NotAllowed)>]
    abstract MyMethod : unit -> unit
        

type MyService() =
    interface IMyContract with
        [<OperationBehavior(TransactionScopeRequired = false)>]
        member this.MyMethod() =
            printTrans()

// The None transaction mode means the service never has a transaction

let host = new InProcHost<MyService>()
let binding = new NetNamedPipeBinding(TransactionFlow = false)
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
