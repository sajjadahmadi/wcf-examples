#light
#r "System.ServiceModel"
#r "System.Runtime.Serialization"
open System
open System.ServiceModel
open System.ServiceModel.Channels
open System.ServiceModel.Description


[<ServiceContract(SessionMode = SessionMode.Required)>]
type IMyContract =
    [<OperationContract>]
    abstract MyMethod : unit -> unit


type MyService() =
    interface IMyContract with
        member this.MyMethod() = printfn "%s" "MyService.MyMethod()"


let uri = new Uri("net.tcp://localhost")
let binding = new NetTcpBinding(SendTimeout = new TimeSpan(0, 0, 5))
let host = new ServiceHost(typeof<MyService>, [| uri |])
host.AddServiceEndpoint(typeof<IMyContract>, binding, "")
let throttle = new ServiceThrottlingBehavior(MaxConcurrentSessions = 1)
host.Description.Behaviors.Add(throttle)
host.Open()

let proxy1 = ChannelFactory<IMyContract>.CreateChannel(binding, new EndpointAddress(string uri)) 
proxy1.MyMethod()

let proxy2 = ChannelFactory<IMyContract>.CreateChannel(binding, new EndpointAddress(string uri)) 
try
    proxy2.MyMethod()
with ex -> printfn "\n%s\n" ex.Message

(proxy1 :?> ICommunicationObject).Close()
host.Close()
