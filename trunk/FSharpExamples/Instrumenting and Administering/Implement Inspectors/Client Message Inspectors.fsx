#r "System.ServiceModel"
#r "System.Runtime.Serialization"
#r @"..\..\bin\Mcts70_503.dll"
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


example<MyService, IMyContract> (fun host _ ->
    let factory = host.CreateChannelFactory()
    let inspector = new PrintToConsoleMessageInspector()
    let behavior = new ApplyClientMessageInspectorBehavior(inspector)
    factory.Endpoint.Behaviors.Add(behavior)
    let proxy = factory.CreateChannel()

    proxy.MyMethod()

    (proxy :?> ICommunicationObject).Close())
