#light
#r "System.ServiceModel"
#r "System.Runtime.Serialization"
#r "System.Transactions"
open System
open System.ServiceModel
open System.ServiceModel.Channels
open System.Transactions
open System.Diagnostics


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


let uri = new Uri("net.tcp://localhost")
let host = new ServiceHost(typeof<MyService>, [| uri |])
let binding = new NetTcpBinding(TransactionFlow = true)
host.AddServiceEndpoint(typeof<IMyContract>, binding, "")
host.Open()

let proxy = ChannelFactory<IMyContract>.CreateChannel(binding, new EndpointAddress(string uri))

let scope = new TransactionScope()
Transaction.Current.TransactionCompleted.Add(fun e ->
    printfn "Transaction Status: %A" e.Transaction.TransactionInformation.Status)
proxy.MyMethod()
proxy.MyOtherMethod()
scope.Complete()
scope.Dispose()

(proxy :?> ICommunicationObject).Close()
host.Close()
