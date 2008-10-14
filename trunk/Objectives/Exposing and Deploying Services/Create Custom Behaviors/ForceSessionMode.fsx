#light
#r "System.ServiceModel"
#r "System.Runtime.Serialization"
#load "../../ref/InProcFactory.fsx"
open Mcts_70_503
open System
open System.Diagnostics
open System.ServiceModel

[<ServiceContract(SessionMode = SessionMode.Required)>]
type IMyContract =
    [<OperationContract>]
    abstract MyMethod : unit -> unit

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

let fact = new InProcFactory("http://localhost", Binding = new BasicHttpBinding())
let proxy = fact.GetInstance<MyService, IMyContract>()

do proxy.MyMethod()
do proxy.MyMethod()

do fact.CloseInstance(proxy)
