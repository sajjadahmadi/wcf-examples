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

[<ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession)>]
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


// Sessions are supported with net.pipe, net.tcp, and WS HTTP if security
//   or reliable messaging are turned on.
let tcpUri = new Uri("net.tcp://localhost")
let httpUri = new Uri("http://localhost")
let tcpBinding = new NetTcpBinding()
let httpBinding = new BasicHttpBinding()
let host = new ServiceHost(typeof<MyService>, [| tcpUri; httpUri |])
host.AddServiceEndpoint(typeof<IMyContract>, tcpBinding, "")
host.AddServiceEndpoint(typeof<IMyContract>, httpBinding, "")
host.Open()

printfn "Per-Session (Named Pipe Binding)\n----------------------"
let proxy1 = ChannelFactory<IMyContract>.CreateChannel(tcpBinding, new EndpointAddress(string tcpUri))

proxy1.MyMethod()
proxy1.MyMethod()

(proxy1 :?> ICommunicationObject).Close()

printfn "\nPer-Session (Basic HTTP Binding)\n----------------------"
let proxy2 = ChannelFactory<IMyContract>.CreateChannel(httpBinding, new EndpointAddress(string httpUri))

proxy2.MyMethod()
proxy2.MyMethod()

(proxy2 :?> ICommunicationObject).Close()
host.Close()
