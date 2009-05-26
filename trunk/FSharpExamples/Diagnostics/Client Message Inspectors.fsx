#r "System.ServiceModel"
#r "System.Runtime.Serialization"
open System
open System.ServiceModel
open System.ServiceModel.Description
open System.ServiceModel.Dispatcher


[<ServiceContract>]
type IMyContract =
    [<OperationContract>]
    abstract MyMethod : unit -> unit


type MyService() =
    interface IMyContract with
        member this.MyMethod() = ()


// Message Inspectors on the service side implement IDispatchMessageInspector
type PrintToConsoleMessageInspector() =
    interface IClientMessageInspector with
        member this.BeforeSendRequest(request, channel) =
            printfn "========\nRequest\n========\n%A\n" request
            null
        
        member this.AfterReceiveReply(reply, correlationState) =
            printfn "========\nReply\n========\n%A\n" reply


// Message Inspectors are added to an endpoint through a Behavior class
type ApplyMessageInspectorBehavior() =
    interface IEndpointBehavior with
        member this.AddBindingParameters(endpoint, bindingParameters) = ()
        member this.ApplyDispatchBehavior(endpoint, endpointDispatcher) = ()
        member this. Validate(endpoint) = ()        
        member this.ApplyClientBehavior(endpoint, clientRuntime) =
            let insp = new PrintToConsoleMessageInspector()
            clientRuntime.MessageInspectors.Add(insp)


let uri = new Uri("net.tcp://localhost")
let binding = new NetTcpBinding()
let host = new ServiceHost(typeof<MyService>, uri)
host.AddServiceEndpoint(typeof<IMyContract>, binding, "")
host.Open()

let factory = new ChannelFactory<IMyContract>(binding, string uri)
// Behavior must be added to endpoint for Message Inspector to be used
factory.Endpoint.Behaviors.Add(new ApplyMessageInspectorBehavior())
let proxy = factory.CreateChannel()

proxy.MyMethod()

(proxy :?> ICommunicationObject).Close()
host.Close()
