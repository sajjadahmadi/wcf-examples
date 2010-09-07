#r "System.ServiceModel"
open System
open System.ServiceModel


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


let host = new ServiceHost(typeof<MyService>, new Uri("http://localhost"))
host.Open()

let proxy = ChannelFactory<IMyContract>.CreateChannel(host.Description.Endpoints.[0].Binding, host.Description.Endpoints.[0].Address)

proxy.MyMethod()
proxy.MyMethod()

(proxy :?> ICommunicationObject).Close()
host.Close()
