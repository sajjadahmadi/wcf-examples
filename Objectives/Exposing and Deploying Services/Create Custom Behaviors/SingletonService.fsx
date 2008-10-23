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

let host = new InProcHost<MyService>()
host.AddEndPoint<IMyContract>(new BasicHttpBinding())
host.Open()

let proxy = host.CreateProxy<IMyContract>()

proxy.MyMethod()
proxy.MyMethod()
proxy.MyMethod()

host.CloseProxy(proxy)
host.Close()
