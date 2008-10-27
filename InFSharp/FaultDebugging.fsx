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
    [<FaultContract(typeof<ExceptionDetail>)>]
    abstract MethodWithError : unit -> unit


type MyService() =
    interface IMyContract with
        member this.MethodWithError() =
            try
                raise (new InvalidOperationException("Some error"))
            with ex ->
                let detail = new ExceptionDetail(ex)
                raise (new FaultException<ExceptionDetail>(detail, ex.Message))


let host = new InProcHost<MyService>()
host.AddEndpoint<IMyContract>()
host.Open()

let proxy = host.CreateProxy<IMyContract>()
try
    proxy.MethodWithError()
with
    | :? FaultException<ExceptionDetail> as ex ->
        printfn "%s: %s" ex.Detail.Type ex.Detail.Message
        printfn "%s" ex.Detail.StackTrace

host.CloseProxy(proxy)
host.Close()