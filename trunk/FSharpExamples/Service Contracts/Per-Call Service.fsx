#light
#r "System.ServiceModel"
#r "System.Runtime.Serialization"
open System
open System.Diagnostics
open System.ServiceModel
open System.ServiceModel.Channels


[<ServiceContract>]
type IMyContract =
    [<OperationContract>]
    abstract MyMethod : unit -> unit


[<ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)>]
type MyService() =
    let mutable counter = 0
    
    do printfn "MyService.MyService()"
    
    interface IMyContract with
        member this.MyMethod() =
            counter <- counter + 1
            printfn "Counter = %d" counter
    
    interface IDisposable with
        member this.Dispose() =
            printfn "MyService.Dispose()"


let uri = new Uri("net.tcp://localhost")
let binding = new NetTcpBinding()
let host = new ServiceHost(typeof<MyService>, [| uri |])
host.AddServiceEndpoint(typeof<IMyContract>, binding, "")
host.Open()

let proxy = ChannelFactory<IMyContract>.CreateChannel(binding, new EndpointAddress(string uri))

proxy.MyMethod()
proxy.MyMethod()

(proxy :?> ICommunicationObject).Close()
host.Close()
