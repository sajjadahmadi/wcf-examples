#r "System.ServiceModel"
#r "System.Runtime.Serialization"
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
// Try example with next line on and off and compare stack traces
[<CallbackBehavior(IncludeExceptionDetailInFaults = true)>]
type MyService() =
    interface IMyContract with
        member this.DoFaultException() =
            let callback = OperationContext.Current.GetCallbackChannel<IMyContractCallback>()
            callback.ThrowFaultException()
        
        member this.DoOtherException() =
            let callback = OperationContext.Current.GetCallbackChannel<IMyContractCallback>()
            callback.ThrowOtherException()


type MyContractClient(callbackInstance: obj, binding: Binding, remoteAddress: EndpointAddress) =
    inherit DuplexClientBase<IMyContract>(callbackInstance, binding, remoteAddress)


type MyCallbackClient() =
    interface IMyContractCallback with
        member this.ThrowFaultException() =
            raise (new FaultException("Some Callback Exception"))
        
        member this.ThrowOtherException() =
            failwith "Some Other Exception"
        

let uri = new Uri("net.tcp://localhost")
let binding = new NetTcpBinding()
let host = new ServiceHost(typeof<MyService>, [| uri |])
host.AddServiceEndpoint(typeof<IMyContract>, binding, "")
host.Open()

let callback = new MyCallbackClient()
let client = new MyContractClient(callback, binding, new EndpointAddress(string uri))
let proxy = client.ChannelFactory.CreateChannel()

proxy.DoFaultException()
printfn "Proxy state: %A\n\n" (proxy :?> ICommunicationObject).State

// With TCP binding, when the callback throws an exception not in the contract,
//   the client that called the service in the first place immediately receives
//   a CommunicationException, *even if the service catches the exception*
try
    proxy.DoOtherException()
with ex ->
    printfn "%s: %s\n\n" (ex.GetType().Name) ex.Message
    
printfn "Proxy state: %A\n\n" (proxy :?> ICommunicationObject).State

(proxy :?> ICommunicationObject).Close()
host.Close()
