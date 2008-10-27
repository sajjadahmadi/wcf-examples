#light
#r "System.ServiceModel"
#r "System.Runtime.Serialization"
#load "InProcHost.fsx"
open Mcts_70_503
open System
open System.Runtime.Serialization
open System.ServiceModel


[<ServiceContract>]
type IMyContract =
    [<OperationContract>]
    abstract MethodWithError : unit -> unit


[<ServiceBehavior(IncludeExceptionDetailInFaults = true)>]
type MyService() =
    interface IMyContract with
        member this.MethodWithError() =
            raise (new InvalidOperationException("Some error"))


let host = new InProcHost<MyService>()
host.AddEndpoint<IMyContract>()
host.Open()

let proxy = host.CreateProxy<IMyContract>()
try
    proxy.MethodWithError()
with ex -> printfn "%s: %s\n%s" (ex.GetType().Name) ex.Message ex.StackTrace

try
    host.CloseProxy(proxy)
with _ -> ()
host.Close()