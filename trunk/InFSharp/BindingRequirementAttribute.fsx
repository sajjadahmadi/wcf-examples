#light
#r "System.ServiceModel"
#r "System.Transactions"
#load "InProcHost.fsx"
open System
open System.ServiceModel
open System.Transactions
open System.ServiceModel.Description
open System.Diagnostics
open Mcts_70_503

[<ServiceContract>]
type IMyContract =
    [<OperationContract>]
    [<TransactionFlow(TransactionFlowOption.Allowed)>]
    abstract MyMethod : unit -> unit


type MyService() =
    interface IMyContract with
        [<OperationBehavior(TransactionScopeRequired = true)>]
        member this.MyMethod() =
            printfn "Transaction.Current = %A" Transaction.Current
            

[<AttributeUsage(AttributeTargets.Class)>]
type BindingRequirementAttribute() =
    inherit Attribute()
    
    let mutable flowEnabled = false
    member this.TransactionFlowEnabled with get() = flowEnabled
                                       and set v = flowEnabled <- v
    
    interface IServiceBehavior with
        member this.AddBindingParameters(description, host, endpoints, bindingParams) =
            ()
        
        member this.ApplyDispatchBehavior(description, host) =
            ()
        
        member this.Validate(description, host) =
            if this.TransactionFlowEnabled = false then ()
            else for endpoint in description.Endpoints do
                let exception = new InvalidOperationException()
                for operation in endpoint.Contract.Operations do
                    for behavior in operation.Behaviors do
                        if behavior
    
    
let host = new InProcHost<MyService>()
host.AddEndpoint<IMyContract>()
host.Open()

let proxy = host.CreateProxy<IMyContract>()
proxy.MyMethod()

host.CloseProxy(proxy)
host.Close()
