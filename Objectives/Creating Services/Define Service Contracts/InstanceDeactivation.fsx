#light
#r "System.ServiceModel"
#r "System.Runtime.Serialization"
#load "../../ref/InProcHost.fsx"
open Mcts_70_503
open System
open System.Diagnostics
open System.ServiceModel

[<ServiceContract(SessionMode = SessionMode.Required)>]
type IMyContract =
    [<OperationContract>]
    abstract MyMethod : unit -> unit
    
    [<OperationContract>]
    abstract MyOtherMethod : unit -> unit

[<ServiceBehavior>]
type MyService() =
    interface IMyContract with
        // Try this example with and without the following line
        [<OperationBehavior(ReleaseInstanceMode = ReleaseInstanceMode.AfterCall)>]
        member this.MyMethod() = ()
        
        member this.MyOtherMethod() = ()

    interface IDisposable with
        member this.Dispose() =
            printfn "Disposing..."

let host = new InProcHost<MyService>()
host.AddEndPoint<IMyContract>()
host.Open()

let proxy = host.CreateProxy<IMyContract>()
printfn "proxy.MyOtherMethod()"
proxy.MyOtherMethod()
printfn "proxy.MyMethod()"
proxy.MyMethod()
System.Threading.Thread.Sleep(100)
printfn "-----------------"

host.CloseProxy(proxy)
host.Close()
