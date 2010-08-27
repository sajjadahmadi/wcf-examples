#r "System.ServiceModel"
#r "System.Runtime.Serialization"
open System
open System.ServiceModel
open System.ServiceModel.Channels
open System.ServiceModel.Description
open System.Threading
open System.Threading.Tasks


[<ServiceContract(SessionMode = SessionMode.Required)>]
type IMyContract =
    [<OperationContract>]
    abstract MyMethod : unit -> unit


type MyService() =
    interface IMyContract with
        member this.MyMethod() = printfn "MyService.MyMethod(): %s" (DateTime.Now.ToLongTimeString())


let uri = new Uri("net.tcp://localhost")
let binding = new NetTcpBinding()
let host = new ServiceHost(typeof<MyService>, uri)
host.AddServiceEndpoint(typeof<IMyContract>, binding, "")
let throttle = new ServiceThrottlingBehavior(MaxConcurrentInstances = 1)

// Comment the following line to turn throttling off
host.Description.Behaviors.Add(throttle)

host.Open()

let proxy1 = ChannelFactory<IMyContract>.CreateChannel(binding, new EndpointAddress(string uri))
let task1 = Task.Factory.StartNew(fun () ->
    proxy1.MyMethod()
    // When thread sleeps, task2 waits on task1 when throttling is enabled
    Thread.Sleep(6000)
    (proxy1 :?> ICommunicationObject).Close())

let proxy2 = ChannelFactory<IMyContract>.CreateChannel(binding, new EndpointAddress(string uri))
let task2 = Task.Factory.StartNew(fun () ->
    try
        proxy2.MyMethod()
        (proxy2 :?> ICommunicationObject).Close()
    with ex -> printfn "\n%s\n" ex.Message)

// When throttling is enabled, the second call should be 6 seconds
// behind the first call
Task.WaitAll([| task1; task2 |])

host.Close()
