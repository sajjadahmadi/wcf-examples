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
type ICalculator =
    [<OperationContract>]
    abstract Add : double * double -> double
    
    [<OperationContract>]
    [<FaultContract(typeof<DivideByZeroException>)>]
    abstract Divide : double * double -> double

type Calculator() =
    interface ICalculator with
        member this.Add(n1, n2) =
            n1 + n2
            
        member this.Divide(n1, n2) =
            if n2 = 0.0 then
                let ex = new DivideByZeroException()
                raise (new FaultException<DivideByZeroException>(ex, "n2 is 0"))
            n1 / n2
            
            
type MyErrorHandler() =
    interface IErrorHandler with
        member this.HandleError(error) =
            printfn "MyErrorHandler.HandleError()"
            false
        
        member this.ProvideFault(error, version, fault) =
            printfn "MyErrorHandler.ProvideFault(): %A" fault
            fault <- null


type ErrorServiceBehavior() =
    interface IServiceBehavior with
        member this.AddBindingParameters(serviceDescription, serviceHostBase, endpoints, bindingParameters) =
            ()
            
        member this.ApplyDispatchBehavior(serviceDescription, serviceHostBase) =
            for cd in serviceHostBase.ChannelDispatchers do
                (cd :?> ChannelDispatcher).ErrorHandlers.Add(new MyErrorHandler())

        member this.Validate(serviceDescription, serviceHostBase) =
            ()


let host = new InProcHost<Calculator>()
host.AddEndpoint<ICalculator>()
// Enable and disable the following line to see the difference
host.InnerHost.Description.Behaviors.Add(new ErrorServiceBehavior())
host.Open()

let proxy = host.CreateProxy<ICalculator>()
printfn "%f / %f = %f" 4.0 2.0 (proxy.Divide(4.0, 2.0))

try
    proxy.Divide(4.0, 0.0) |> ignore
with ex -> 
    printfn "%A: %s" (ex.GetType()) ex.Message

// When the service throws an exception listed in the service-side fault
//  contract, the exception will not fault the communication channel.
printfn "Proxy state = %A" (proxy :?> ICommunicationObject).State

host.CloseProxy(proxy)
host.Close()

