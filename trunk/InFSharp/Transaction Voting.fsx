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


[<ServiceContract(SessionMode = SessionMode.Required)>]
type IMyContract =
    [<OperationContract>]
    [<TransactionFlow(TransactionFlowOption.Allowed)>]
    abstract MyMethod : unit -> unit
    
    [<OperationContract>]
    [<TransactionFlow(TransactionFlowOption.Allowed)>]
    abstract MyOtherMethod : unit -> unit
        

type MyService() =
    interface IMyContract with
        // DECLARATIVE VOTING
        [<OperationBehavior(TransactionScopeRequired = true, TransactionAutoComplete = true)>]
        // It is very important when TransactionScopeRequired = true to avoid
        //   catching and handling exceptions and explicitly voting to abort
        // If you want to catch the exception for some local handling, make
        //   sure to rethrow it
        member this.MyMethod() =
            try
                // some work
                ()
            with ex -> raise ex 

        // EXPLICIT VOTING
        [<OperationBehavior(TransactionScopeRequired = true, TransactionAutoComplete = false)>]
        member this.MyOtherMethod() =
            try
                // some work
                OperationContext.Current.SetTransactionComplete()
            with ex -> raise ex


let host = new InProcHost<MyService>()
let binding = new NetNamedPipeBinding(TransactionFlow = true)
host.AddEndpoint<IMyContract>(binding)
host.IncludeExceptionDetailInFaults <- true
host.Open()
let proxy = host.CreateProxy<IMyContract>()

let scope = new TransactionScope()
proxy.MyMethod()
proxy.MyOtherMethod()
scope.Complete()
scope.Dispose()

host.CloseProxy(proxy)
host.Close()
