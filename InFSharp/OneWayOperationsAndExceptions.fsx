#light
#r "System.ServiceModel"
#r "System.Runtime.Serialization"
#load "InProcHost.fsx"
open Mcts_70_503
open System
open System.ServiceModel
open System.ServiceModel.Description

[<ServiceContract>]
type IMyContract =
    [<OperationContract(IsOneWay = true)>]
    abstract MethodWithError : unit -> unit
    
    [<OperationContract>]
    abstract MethodWithoutError : unit -> unit

type MyService() =
    
    interface IMyContract with
        member this.MethodWithError() = failwith "BOOM!"
        
        member this.MethodWithoutError() = printfn "MyService.MethodWithoutError()"

let host = new InProcHost<MyService>()
host.AddEndPoint<IMyContract>()
host.Open()

let proxy = host.CreateProxy<IMyContract>()
proxy.MethodWithError()
try
    // will error because channel is at fault
    proxy.MethodWithoutError()    
with ex -> printfn "\n%s\n" ex.Message

printfn "%A" host.InnerHost.State

host.Close()
