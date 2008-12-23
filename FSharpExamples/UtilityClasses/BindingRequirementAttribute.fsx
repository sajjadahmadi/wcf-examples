#light
#r "System.ServiceModel"
#r "System.Runtime.Serialization"
#r "System.Transactions"
open System
open System.ServiceModel
open System.Transactions
open System.ServiceModel.Channels
open System.ServiceModel.Description
open System.Diagnostics


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
                let ex = new InvalidOperationException()
                for operation in endpoint.Contract.Operations do
                    for behavior in operation.Behaviors do
                        () //if behavior
                        
                        
[<ServiceContract>]
[<BindingRequirement>]
type IMyContract =
    [<OperationContract>]
    [<TransactionFlow(TransactionFlowOption.Allowed)>]
    abstract MyMethod : unit -> unit


type MyService() =
    interface IMyContract with
        [<OperationBehavior(TransactionScopeRequired = true)>]
        member this.MyMethod() =
            printfn "Transaction.Current = %A" Transaction.Current
            

let uri = new Uri("net.tcp://localhost")
let binding = new NetTcpBinding()
let host = new ServiceHost(typeof<MyService>, [| uri |])
host.AddServiceEndpoint(typeof<IMyContract>, binding, "")
host.Open()

let proxy = ChannelFactory<IMyContract>.CreateChannel(binding, new EndpointAddress(string uri))
proxy.MyMethod()

(proxy :?> ICommunicationObject).Close()
host.Close()
