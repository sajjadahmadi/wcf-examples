#light
#r "System.ServiceModel"
#r "System.Runtime.Serialization"
#load "InProcHost.fsx"
open Mcts_70_503
open System
open System.ServiceModel
open System.ServiceModel.Channels

type IMyContractCallback =
    [<OperationContract>]
    abstract ThrowFaultException : unit -> unit

    [<OperationContract>]
    abstract ThrowOtherException : unit -> unit
    
    
[<ServiceContract(CallbackContract = typeof<IMyContractCallback>)>]
type IMyContract =
    [<OperationContract>]
    abstract DoFaultException : unit -> unit
    
    [<OperationContract>]
    abstract DoOtherException : unit -> unit


[<ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Reentrant)>]
type MyService() =
    interface IMyContract with
        member this.DoFaultException() =
            let callback = OperationContext.Current.GetCallbackChannel<IMyContractCallback>()
            try
                callback.ThrowFaultException()
            with :? FaultException as ex ->
                printfn "%s: %s" (ex.GetType().Name) ex.Message
        
        member this.DoOtherException() =
            let callback = OperationContext.Current.GetCallbackChannel<IMyContractCallback>()
            try
                callback.ThrowOtherException()
            with ex ->
                printfn "%s: %s" (ex.GetType().Name) ex.Message

type MyContractClient(callbackInstance: obj, binding: Binding, remoteAddress: EndpointAddress) =
    inherit DuplexClientBase<IMyContract>(callbackInstance, binding, remoteAddress)


type MyCallbackClient() =
    interface IMyContractCallback with
        member this.ThrowFaultException() =
            raise (new FaultException("Some Callback Exception"))
        
        member this.ThrowOtherException() =
            failwith "Some Other Exception"
        
        
let host = new InProcHost<MyService>()
host.AddEndpoint<IMyContract>()
host.Open()

let callback = new MyCallbackClient()
let client = new MyContractClient(callback, new NetNamedPipeBinding(), new EndpointAddress("net.pipe://localhost"))
let proxy = client.ChannelFactory.CreateChannel()

proxy.DoFaultException()
printfn "Proxy state: %A\n\n" (proxy :?> ICommunicationObject).State

// With IPC binding, when the callback throws an exception not in the contract,
//   the client that called the service in the first place immediately receives
//   a CommunicationException, *even if the service catches the exception*
try
    proxy.DoOtherException()
with ex ->
    printfn "%s: %s" (ex.GetType().Name) ex.Message
printfn "Proxy state: %A\n\n" (proxy :?> ICommunicationObject).State

host.CloseProxy(proxy)
host.Close()
