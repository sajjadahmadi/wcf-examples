#light
#r "System.ServiceModel"
#r "System.Runtime.Serialization"
#load "InProcHost.fsx"
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
        // Try this example with none or one of the following lines
        //[<OperationBehavior(ReleaseInstanceMode = ReleaseInstanceMode.BeforeCall)>]
        //[<OperationBehavior(ReleaseInstanceMode = ReleaseInstanceMode.AfterCall)>]
        //[<OperationBehavior(ReleaseInstanceMode = ReleaseInstanceMode.BeforeAndAfterCall)>]
        member this.MyMethod() = printfn "proxy.MyMethod()"; System.Threading.Thread.Sleep(100)
        
        member this.MyOtherMethod() = printfn "proxy.MyOtherMethod()"; System.Threading.Thread.Sleep(100)

    interface IDisposable with
        member this.Dispose() =
            printfn "Disposing..."

let host = new InProcHost<MyService>()
host.AddEndPoint<IMyContract>()
host.Open()

let proxy = host.CreateProxy<IMyContract>()

proxy.MyOtherMethod()
proxy.MyMethod()
System.Threading.Thread.Sleep(100)
printfn "-----------------"

host.CloseProxy(proxy)
host.Close()
