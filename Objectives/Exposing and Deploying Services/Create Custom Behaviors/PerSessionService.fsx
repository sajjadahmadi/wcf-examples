#light
#r "System.ServiceModel"
#r "System.Runtime.Serialization"
#load "../../ref/InProcHost.fsx"
open Mcts_70_503
open System
open System.Diagnostics
open System.ServiceModel

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

let host = new InProcHost<MyService>()
host.AddEndPoint<IMyContract>(new NetNamedPipeBinding())
host.AddEndPoint<IMyContract>(new BasicHttpBinding())
host.Open()

printfn "Per-Session (Named Pipe Binding)\n----------------------"
let mutable proxy = host.CreateProxy<IMyContract>()

proxy.MyMethod()
proxy.MyMethod()

host.CloseProxy(proxy)

printfn "\nPer-Session (Basic HTTP Binding)\n----------------------"
proxy <- host.CreateProxy<IMyContract, BasicHttpBinding>()

proxy.MyMethod()
proxy.MyMethod()

host.CloseProxy(proxy)
host.Close()
