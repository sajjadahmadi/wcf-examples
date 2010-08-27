#r "System.ServiceModel"
#r "System.Runtime.Serialization"
open System
open System.ServiceModel
open System.ServiceModel.Channels


type IMyContractCallback =
    [<OperationContract>]
    abstract OnCallback : unit -> unit


[<ServiceContract(CallbackContract = typeof<IMyContractCallback>)>]
type IMyContract =
    [<OperationContract>]
    abstract DoSomething : unit -> unit


[<ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Reentrant)>]
type MyService() =
    interface IMyContract with
        member this.DoSomething() =
            printfn "MyService.DoSomething()"
            let callback = OperationContext.Current.GetCallbackChannel<IMyContractCallback>()
            callback.OnCallback()


type MyCallbackClient() =
    interface IMyContractCallback with
        member this.OnCallback() = printfn "MyCallbackClient.OnCallback()"


type MyContractClient(callbackInstance: obj, binding: Binding, remoteAddress: EndpointAddress) =
    inherit DuplexClientBase<IMyContract>(callbackInstance, binding, remoteAddress)


let uri = new Uri("http://localhost")
let binding = new WSDualHttpBinding()
let host = new ServiceHost(typeof<MyService>, uri)
host.AddServiceEndpoint(typeof<IMyContract>, binding, "")
host.Open()

let callback = new MyCallbackClient()
let client = new MyContractClient(callback, binding, new EndpointAddress(string uri))
let proxy = client.ChannelFactory.CreateChannel()
proxy.DoSomething()

(proxy :?> ICommunicationObject).Close()
host.Close()
