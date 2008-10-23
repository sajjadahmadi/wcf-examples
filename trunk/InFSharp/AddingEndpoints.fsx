#light
#r "System.ServiceModel"
#r "System.Runtime.Serialization"
open System
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

let mutable host = new ServiceHost(typeof<MyService>, [| new Uri("net.pipe://localhost") |])

try
    // Can't start a service that has no endpoints
    host.Open()
with ex -> printfn "%s" ex.Message

printfn "\nService now has %A state..." host.State


host <- new ServiceHost(typeof<MyService>, [| new Uri("net.pipe://localhost"); new Uri("net.tcp://localhost") |])
let binding = new NetNamedPipeBinding()
host.AddServiceEndpoint(typeof<IMyContract>, binding, "")
host.Open()

let binding2 = new NetTcpBinding()
try
    host.AddServiceEndpoint(typeof<IMyContract>, binding2, "") |> ignore
with ex -> printfn "\n%s" ex.Message
