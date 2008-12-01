#light
#r "System.ServiceModel"
#r "System.Runtime.Serialization"
#load "InProcHost.fsx"
open System.ServiceModel
open System.Threading
open Mcts_70_503

type MyResource() =
    let context = new SynchronizationContext()
    
    member this.DoWork() =
        printfn "MyResource.DoWork(): %d" Thread.CurrentThread.ManagedThreadId
    
    member this.SynchronizationContext = context


[<ServiceContract>]
type IMyContract =
    [<OperationContract>]
    abstract MyMethod : unit -> unit


type MyService() =
    let resource = new MyResource()
    
    interface IMyContract with
        member this.MyMethod() =
            printfn "MyService.MyMethod(): %d" Thread.CurrentThread.ManagedThreadId
            let context = resource.SynchronizationContext
            context.Send((fun state ->
                resource.DoWork()
                printfn "doWork(): %d" Thread.CurrentThread.ManagedThreadId), null)

printfn "main(): %d" Thread.CurrentThread.ManagedThreadId
let host = new InProcHost<MyService>()
host.AddEndpoint<IMyContract>()
host.Open()

let proxy = host.CreateProxy<IMyContract>()
proxy.MyMethod()
