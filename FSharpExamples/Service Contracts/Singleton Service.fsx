#r "System.ServiceModel"
#r "System.Runtime.Serialization"
open System
open System.ServiceModel


[<ServiceContract>]
type IMyContract =
    [<OperationContract>]
    abstract MyMethod : unit -> unit


// Try the following to highlight the differences
//[<ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession)>]
[<ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)>]
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


let uri = new Uri("http://localhost")
let binding = new BasicHttpBinding()
let host = new ServiceHost(typeof<MyService>, uri)
host.AddServiceEndpoint(typeof<IMyContract>, binding, "")
host.Open()

let proxy = ChannelFactory<IMyContract>.CreateChannel(binding, new EndpointAddress(string uri))

proxy.MyMethod()
proxy.MyMethod()
proxy.MyMethod()

(proxy :?> ICommunicationObject).Close()
host.Close()
