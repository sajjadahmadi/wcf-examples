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


[<ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)>]
type MySingleton() =
    let mutable counter = 0
    
    member this.Counter with get() = counter
                        and set v = counter <- v
    
    interface IMyContract with
        member this.MyMethod() =
            counter <- counter + 1
            printfn "Counter = %d" counter


let singleton = new MySingleton()
singleton.Counter <- 42

let uri = new Uri("net.tcp://localhost")
let binding = new NetTcpBinding()
let host = new ServiceHost(singleton, [| uri |])
host.AddServiceEndpoint(typeof<IMyContract>, 
host.Open()

let proxy = host.CreateProxy<IMyContract>()

proxy.MyMethod()
proxy.MyMethod()
proxy.MyMethod()

host.CloseProxy(proxy)
host.Close()