#r "System.ServiceModel"
#r "System.Runtime.Serialization"
#r @"..\bin\Mcts70_503.dll"
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

[<PrintMessagesToConsole>]
type MyService() =
    interface ISomeRpcService2 with
        member this.SomeOp2(name) =
            sprintf "Hi, %s!" name


//type PrintToConsoleMessageInspector() =
//    interface IDispatchMessageInspector with
//        member this.AfterReceiveRequest(request, channel, instanceContext) =
//            printfn "========\nRequest\n========\n%A\n" request
//            null
//        
//        member this.BeforeSendReply(reply, correlationState) =
//            printfn "========\nReply\n========\n%A\n" reply


//type ApplyMessageInspectorBehavior() =
//    interface IEndpointBehavior with
//        member this.AddBindingParameters(endpoint, bindingParameters) = ()
//        member this.ApplyClientBehavior(endpoint, clientRuntime) = ()
//        member this. Validate(endpoint) = ()
//        member this.ApplyDispatchBehavior(endpoint, endpointDispatcher) =
//            let insp = new PrintToConsoleMessageInspector()
//            endpointDispatcher.DispatchRuntime.MessageInspectors.Add(insp)
            
            
let host = new ExampleHost<MyService, ISomeRpcService2>()
//host.Description.Endpoints.[0].Behaviors.Add(new ApplyMessageInspectorBehavior())
host.Open()

let proxy = host.CreateProxy()
printfn "%s" (proxy.SomeOp2("Ray"))

host.Close()
