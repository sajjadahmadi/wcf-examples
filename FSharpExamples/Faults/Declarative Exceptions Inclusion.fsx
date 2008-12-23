#light
#r "System.ServiceModel"
#r "System.Runtime.Serialization"
open System
open System.Runtime.Serialization
open System.ServiceModel
open System.ServiceModel.Channels


[<ServiceContract>]
type IMyContract =
    [<OperationContract>]
    abstract MethodWithError : unit -> unit


[<ServiceBehavior(IncludeExceptionDetailInFaults = true)>]
type MyService() =
    interface IMyContract with
        member this.MethodWithError() =
            raise (new InvalidOperationException("Some error"))


let uri = new Uri("net.tcp://localhost")
let binding = new NetTcpBinding()
let host = new ServiceHost(typeof<MyService>, [| uri |])
host.AddServiceEndpoint(typeof<IMyContract>, binding, "")
host.Open()

let proxy = ChannelFactory<IMyContract>.CreateChannel(binding, new EndpointAddress(string uri))
try
    proxy.MethodWithError()
with ex -> printfn "%s: %s\n%s" (ex.GetType().Name) ex.Message ex.StackTrace

try
    (proxy :?> ICommunicationObject).Close()
with _ -> ()
host.Close()