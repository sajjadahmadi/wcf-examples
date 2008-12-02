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
    abstract MySynchronizedMethod : unit -> unit
    
    [<OperationContract>]
    abstract MyUnsynchronizedMethod : unit -> unit


[<ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)>]
type MyService(resource : MyResource) =
    interface IMyContract with
        member this.MySynchronizedMethod() =
            printfn "MyService.MySynchronizedMethod(): %d" Thread.CurrentThread.ManagedThreadId
            let context = resource.SynchronizationContext
            context.Send((fun state ->
                resource.DoWork()), null)

        member this.MyUnsynchronizedMethod() =
            printfn "MyService.MyUnsynchronizedMethod(): %d" Thread.CurrentThread.ManagedThreadId


printfn "main(): %d" Thread.CurrentThread.ManagedThreadId
let resource = new MyResource()
resource.DoWork()
let service = new MyService(resource)
let host = new InProcHost<MyService>(service)
host.AddEndpoint<IMyContract>()
host.Open()

let proxy = host.CreateProxy<IMyContract>()
proxy.MySynchronizedMethod()
proxy.MyUnsynchronizedMethod()

host.CloseProxy(proxy)
host.Close()
