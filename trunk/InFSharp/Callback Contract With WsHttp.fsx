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

let host = new InProcHost<MyService>()
host.AddEndpoint<IMyContract>(new WSDualHttpBinding())
host.Open()

let callback = new MyCallbackClient()
let client = new MyContractClient(callback, new WSDualHttpBinding(), new EndpointAddress("http://localhost"))
let proxy = client.ChannelFactory.CreateChannel()
proxy.DoSomething()

host.CloseProxy(proxy)
host.Close()
