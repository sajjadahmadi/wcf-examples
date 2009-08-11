#r "System.ServiceModel"
#r "System.Runtime.Serialization"
open System
open System.ServiceModel
open System.ServiceModel.Channels
open System.ServiceModel.Description
open System.ServiceModel.Dispatcher


module Printer =
    let private l = new obj()
    
    let print format (args : #obj[]) =
        let args = Array.ConvertAll(args, fun x -> box x)
        let f() = Console.WriteLine(format, args)
        lock l f


[<AttributeUsage(AttributeTargets.Class)>]
type ErrorHandlerBehaviorAttribute() =
    inherit Attribute()
    
    interface IServiceBehavior with
        member this.AddBindingParameters(serviceDescription, serviceHostBase, endpoints, bindingParameters) =
            ()
            
        member this.ApplyDispatchBehavior(serviceDescription, serviceHostBase) =
            for cd in serviceHostBase.ChannelDispatchers do
                (cd :?> ChannelDispatcher).ErrorHandlers.Add(this)

        member this.Validate(serviceDescription, serviceHostBase) =
            ()
            
            
    interface IErrorHandler with
        member this.HandleError(error) =
            Printer.print "MyErrorHandler.HandleError(): {0}" [| error.Message |]
            false
        
        member this.ProvideFault(error, version, fault) =
            Printer.print "MyErrorHandler.ProvideFault(): {0}" [| error.Message |]
            let faultException = new FaultException<int>(-99)
            let messageFault = faultException.CreateMessageFault()
            fault <- Message.CreateMessage(version, messageFault, faultException.Action)


[<ServiceContract>]
type ICalculator =
    [<OperationContract>]
    [<FaultContract(typeof<int>)>]
    abstract Divide : double * double -> double


[<ErrorHandlerBehavior>]
type Calculator() =
    interface ICalculator with
        member this.Divide(n1, n2) =
            match n2 with
            | 0.0  ->
                raise (new DivideByZeroException())
            | _    -> n1 / n2


let uri = new Uri("net.tcp://localhost")
let binding = new NetTcpBinding()
let host = new ServiceHost(typeof<Calculator>, [| uri |])
host.AddServiceEndpoint(typeof<ICalculator>, binding, "")
host.Open()

let proxy = ChannelFactory<ICalculator>.CreateChannel(binding, new EndpointAddress(string uri))

try
    proxy.Divide(4.0, 0.0) |> ignore
with 
    | :? FaultException<int> as ex ->
        Printer.print "{0}: {1}\n\n" [| box (ex.GetType()); box ex.Detail |]
    | ex                           -> 
        Printer.print "Expected FailtException<int> but got {0} instead!" [| ex.GetType() |]

Printer.print "proxy.State = {0}" [| (proxy :?> ICommunicationObject).State |]

(proxy :?> ICommunicationObject).Close()
host.Close()
