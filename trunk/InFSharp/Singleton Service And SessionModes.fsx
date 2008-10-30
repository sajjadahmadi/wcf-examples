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

[<ServiceContract(SessionMode = SessionMode.NotAllowed)>]
type IMyOtherContract =
    [<OperationContract>]
    abstract MyOtherMethod : unit -> unit
    

// Try the following to highlight the differences
//[<ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession)>]
[<ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)>]
type MySingleton() =
    let mutable counter = 0
    
    do printfn "MySingleton.MySingleton()"
    
    interface IMyContract with
        member this.MyMethod() =
            counter <- counter + 1
            printfn "Counter = %d" counter
    
    interface IMyOtherContract with
        member this.MyOtherMethod() =
            counter <- counter + 1
            printfn "Counter = %d" counter
    
    interface IDisposable with
        member this.Dispose() =
            printfn "MySingleton.Dispose()"

let host = new InProcHost<MySingleton>()
host.AddEndpoint<IMyContract>(new WSHttpBinding(SecurityMode.None, true))
host.AddEndpoint<IMyOtherContract>(new BasicHttpBinding(), "other")
host.Open()

let proxy1 = host.CreateProxy<IMyContract>()
proxy1.MyMethod()
proxy1.MyMethod()
host.CloseProxy(proxy1)

let proxy2 = host.CreateProxy<IMyOtherContract>()
proxy2.MyOtherMethod()
host.CloseProxy(proxy2)
host.Close()
