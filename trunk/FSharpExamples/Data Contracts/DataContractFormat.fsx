#r "System.ServiceModel"
#r "System.Runtime.Serialization"
open System
open System.ServiceModel
open System.ServiceModel.Description
open System.ServiceModel.Dispatcher
Console.Clear()


[<ServiceContract>]
[<DataContractFormat(Style = OperationFormatStyle.Rpc)>]
type ISomeRpcService2 =
    [<OperationContract>]
    abstract SomeOp2 : string -> string


type MyService() =
    interface ISomeRpcService2 with
        member this.SomeOp2(name) =
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
host.AddServiceEndpoint(typeof<ISomeRpcService2>, binding, "")
host.Description.Endpoints.[0].Behaviors.Add(new ApplyMessageInspectorBehavior())
host.Open()

let proxy = ChannelFactory<ISomeRpcService2>.CreateChannel(binding, new EndpointAddress(string uri))
printfn "%s" (proxy.SomeOp2("Ray"))

(proxy :?> ICommunicationObject).Close()
host.Close()