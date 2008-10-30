#light
#r "System.ServiceModel"
#r "System.Runtime.Serialization"
#load "InProcHost.fsx"
open Mcts_70_503
open System
open System.ServiceModel
open System.ServiceModel.Channels
open System.ServiceModel.Description
open System.ServiceModel.Dispatcher

[<ServiceContract>]
type IMyContract =
    [<OperationContract>]
    [<FaultContract(typeof<InvalidOperationException>)>]
    abstract MyMethod : unit -> unit


type MyService() =
    interface IMyContract with
        member this.MyMethod() =
            raise (new InvalidOperationException())
        
        
type MyErrorHandler() =
    interface IErrorHandler with
        member this.HandleError(error) = false
        
        member this.ProvideFault(error, version, fault) =
            match error with
            | :? InvalidOperationException ->
                let ex = new InvalidOperationException(error.Message)
                let faultException = new FaultException<InvalidOperationException>(ex)
                let messageFault = faultException.CreateMessageFault()
                fault <- Message.CreateMessage(version, messageFault, faultException.Action)
            | _ -> ()


type ErrorServiceBehavior() =
    interface IServiceBehavior with
        member this.AddBindingParameters(serviceDescription, serviceHostBase, endpoints, bindingParameters) = ()
            
        member this.ApplyDispatchBehavior(serviceDescription, serviceHostBase) =
            for cd in serviceHostBase.ChannelDispatchers do
                (cd :?> ChannelDispatcher).ErrorHandlers.Add(new MyErrorHandler())

        member this.Validate(serviceDescription, serviceHostBase) = ()


let host = new InProcHost<MyService>()
host.AddEndpoint<IMyContract>()
// Try this example with the following line enabled or disabled
host.InnerHost.Description.Behaviors.Add(new ErrorServiceBehavior())
host.Open()


let proxy = host.CreateProxy<IMyContract>()

try
    proxy.MyMethod()
with ex ->
    printfn "%A: %s\n\n" (ex.GetType()) ex.Message
    printfn "Proxy state = %A\n\n" (proxy :?> ICommunicationObject).State

try
    host.CloseProxy(proxy)
with _ -> ()
host.Close()

Threading.Thread.Sleep(100)
