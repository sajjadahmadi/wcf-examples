#light
#r "System.ServiceModel"
#r "System.Runtime.Serialization"
#load "InProcHost.fsx"
open System
open System.Windows.Forms
open System.ServiceModel
open Mcts_70_503


[<ServiceContract>]
type IMyContract =
    [<OperationContract>]
    abstract MyMethod : unit -> unit


type MyService() =
    interface IMyContract with
        member this.MyMethod() = printfn "MyService.MyMethod()"


type HostForm() as this =
    inherit Form()
    
    let host = new InProcHost<MyService>()
    do host.AddEndpoint<IMyContract>()
    do this.FormClosed.Add(fun e -> host.Close())
    do host.Open()


Application.Run(new HostForm())
