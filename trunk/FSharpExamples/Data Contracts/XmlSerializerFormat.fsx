#r "System.ServiceModel"
#r "System.Runtime.Serialization"
open System
open System.ServiceModel
open System.ServiceModel.Description
open System.ServiceModel.Dispatcher
Console.Clear()


[<ServiceContract>]
[<XmlSerializerFormat(Style = OperationFormatStyle.Rpc, Use = OperationFormatUse.Encoded)>]
type ISomeLegacyService =
    [<OperationContract>]
    abstract SomeOp1 : string -> string


type MyService() =
    interface ISomeLegacyService with
        member this.SomeOp1(name) =
            sprintf "Hi, %s!" name


type PrintToConsoleMessageInspector() =
    interface IDispatchMessageInspector with
        member this.AfterReceiveRequest(request, channel, instanceContext) =
            printfn "========\nRequest\n========\n%A\n" request
            null
        
        member this.BeforeSendReply(reply, correlationState) =
            printfn "========\nReply\n========\n%A\n" reply


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
host.AddServiceEndpoint(typeof<ISomeLegacyService>, binding, "")
host.Description.Endpoints.[0].Behaviors.Add(new ApplyMessageInspectorBehavior())
host.Open()

let proxy = ChannelFactory<ISomeLegacyService>.CreateChannel(binding, new EndpointAddress(string uri))
printfn "%s" (proxy.SomeOp1("Ray"))

(proxy :?> ICommunicationObject).Close()
host.Close()
