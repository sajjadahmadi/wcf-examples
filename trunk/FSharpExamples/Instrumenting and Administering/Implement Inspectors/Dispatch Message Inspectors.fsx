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
    interface IDispatchMessageInspector with
        member this.AfterReceiveRequest(request, channel, instanceContext) =
            printfn "========\nRequest\n========\n%A\n" request
            null
        
        member this.BeforeSendReply(reply, correlationState) =
            printfn "========\nReply\n========\n%A\n" reply



example2<MyService, IMyContract>
    (fun() ->
        let host = new ExampleHost<MyService, IMyContract>()
        let inspector = new PrintToConsoleMessageInspector()
        let behavior = new ApplyDispatchMessageInspectorBehavior(inspector)
        host.Description.Endpoints.[0].Behaviors.Add(behavior)
        host)
    (fun _ proxy ->
        proxy.MyMethod())
