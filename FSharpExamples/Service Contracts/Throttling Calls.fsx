#light
#r "System.ServiceModel"
#r "System.Runtime.Serialization"
open System
open System.ServiceModel
open System.ServiceModel.Channels
open System.ServiceModel.Description


type DateTime with
    static member CurrentTime =
        let now = DateTime.Now
        sprintf "%d:%d:%d" now.Hour now.Minute now.Second


[<ServiceContract(SessionMode = SessionMode.Required)>]
type IMyContract =
    [<OperationContract>]
    abstract MyMethod : unit -> unit


type MyService() =
    interface IMyContract with
        member this.MyMethod() =
            Threading.Thread.Sleep(6000)
            printfn "%s: %s" "MyService.MyMethod()" DateTime.CurrentTime


let tcpUri = new Uri("net.tcp://localhost")
let pipeUri = new Uri("net.pipe://localhost")
let tcpBinding = new NetTcpBinding()
let pipeBinding = new NetNamedPipeBinding(SendTimeout = new TimeSpan(0, 0, 5))
let host = new ServiceHost(typeof<MyService>, [| tcpUri; pipeUri |])
host.AddServiceEndpoint(typeof<IMyContract>, tcpBinding, "")
host.AddServiceEndpoint(typeof<IMyContract>, pipeBinding, "")
let throttle = new ServiceThrottlingBehavior(MaxConcurrentCalls = 1)
host.Description.Behaviors.Add(throttle)
host.Open()

let proxy1 = ChannelFactory<IMyContract>.CreateChannel(tcpBinding, new EndpointAddress(string tcpUri))
let call1 = async{ proxy1.MyMethod() }
Async.Run call1

let proxy2 = ChannelFactory<IMyContract>.CreateChannel(pipeBinding, new EndpointAddress(string pipeUri))
let call2 = async{ proxy2.MyMethod() }
try
    Async.Run call2
    (proxy2 :?> ICommunicationObject).Close()
with ex -> printfn "\n%s\n" ex.Message

(proxy1 :?> ICommunicationObject).Close()
host.Close()
