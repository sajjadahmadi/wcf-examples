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
            let callback = OperationContext.Current.GetCallbackChannel<IMyContractCallback>()
            callback.OnCallback()


type MyContractClient(callbackInstance: obj, binding: Binding, remoteAddress: EndpointAddress) =
    inherit DuplexClientBase<IMyContract>(callbackInstance, binding, remoteAddress)


type MyCallbackClient() =
    interface IMyContractCallback with
        member this.OnCallback() = printfn "MyCallbackClient.OnCallback()"


let host = new ServiceHost(typeof<MyService>, new Uri("net.tcp://localhost"))
host.Open()

let callback = new MyCallbackClient()
let client = new MyContractClient(callback, host.Description.Endpoints.[0].Binding, host.Description.Endpoints.[0].Address)
let proxy = client.ChannelFactory.CreateChannel()
proxy.DoSomething()

(proxy :?> ICommunicationObject).Close()
host.Close()
