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
    interface IDispatchMessageInspector with
        member this.AfterReceiveRequest(request, channel, instanceContext) =
            printfn "========\nRequest\n========\n%A\n" request
            null
        
        member this.BeforeSendReply(reply, correlationState) =
            printfn "========\nReply\n========\n%A\n" reply


// Message Inspectors are added to an endpoint through a Behavior class
type ApplyMessageInspectorBehavior() =
    interface IEndpointBehavior with
        member this.AddBindingParameters(endpoint, bindingParameters) = ()
        member this.ApplyClientBehavior(endpoint, clientRuntime) = ()
        member this. Validate(endpoint) = ()
        member this.ApplyDispatchBehavior(endpoint, endpointDispatcher) =
            let insp = new PrintToConsoleMessageInspector()
            endpointDispatcher.DispatchRuntime.MessageInspectors.Add(insp)


let uri = new Uri("net.tcp://localhost")
let binding = new NetTcpBinding()
let host = new ServiceHost(typeof<MyService>, uri)
host.AddServiceEndpoint(typeof<IMyContract>, binding, "")
// Behavior must be added to endpoint for Message Inspector to be used
host.Description.Endpoints.[0].Behaviors.Add(new ApplyMessageInspectorBehavior())

host.Open()

let proxy = ChannelFactory<IMyContract>.CreateChannel(binding, new EndpointAddress(string uri))

proxy.MyMethod()

(proxy :?> ICommunicationObject).Close()
host.Close()
