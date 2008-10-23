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

let host = new InProcHost<MySingleton>(singleton)
host.AddEndPoint<IMyContract>()
host.Open()

let proxy = host.CreateProxy<IMyContract>()

proxy.MyMethod()
proxy.MyMethod()
proxy.MyMethod()

host.CloseProxy(proxy)
host.Close()