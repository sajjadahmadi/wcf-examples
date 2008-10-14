#light
#r "System.ServiceModel"
#r "System.Runtime.Serialization"
#load "../../ref/InProcFactory.fsx"
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
printfn "Per-Session (Named Pipe Binding)\n----------------------"
let mutable fact = new InProcFactory()
let mutable proxy = fact.GetInstance<MyService, IMyContract>()

do proxy.MyMethod()
do proxy.MyMethod()

do fact.CloseInstance(proxy)

printfn "\nPer-Session (Basic HTTP Binding)\n----------------------"
fact <- new InProcFactory("http://localhost", Binding = new BasicHttpBinding())
proxy <- fact.GetInstance<MyService, IMyContract>()

do proxy.MyMethod()
do proxy.MyMethod()

do fact.CloseInstance(proxy)