#light
#r "System.Runtime.Serialization"
#r "System.ServiceModel"
#load "InProcHost.fsx"
open Mcts_70_503
open System.ServiceModel
open System.ServiceModel.Channels
open System.ServiceModel.Description

[<ServiceContract>]
type IMyContract =
    [<OperationContract>]
    abstract MyOperation : unit -> string

type MyService() =
    interface IMyContract with
        member this.MyOperation() = "My Message"

let host = new InProcHost<MyService>()
host.AddEndPoint<IMyContract>()
host.Open()

let binding = new NetNamedPipeBinding() :> Binding
let addr = new EndpointAddress("net.pipe://localhost")
let channel = ChannelFactory<IMyContract>.CreateChannel(binding, addr)

let result = channel.MyOperation()
printfn "Result: %s" result
