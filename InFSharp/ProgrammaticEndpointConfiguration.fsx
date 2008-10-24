#light
#r "System.ServiceModel"
open System
open System.ServiceModel
open System.ServiceModel.Channels

[<ServiceContract>]
type IMyContract =
    [<OperationContract>]
    abstract MyOperation : unit -> string

type MyService() =
    interface IMyContract with
        member this.MyOperation() = "MyResult"

let host = new ServiceHost(typeof<MyService>, [| new Uri("http://localhost") |])

let wsBinding = new WSHttpBinding()
let tcpBinding = new NetTcpBinding()

// Can add multiple endpoints of the same type as long as they differ by URL
host.AddServiceEndpoint(typeof<IMyContract>, wsBinding, "http://localhost:8000/MyService")
host.AddServiceEndpoint(typeof<IMyContract>, tcpBinding, "net.tcp://localhost:8001/MyService")
host.AddServiceEndpoint(typeof<IMyContract>, tcpBinding, "net.tcp://localhost:8002/MyService")

// host base address
host.AddServiceEndpoint(typeof<IMyContract>, wsBinding, "")

// relative address
host.AddServiceEndpoint(typeof<IMyContract>, wsBinding, "MyService")

// Absolute address
host.AddServiceEndpoint(typeof<IMyContract>, wsBinding, "http://localhost:8003/MyService")

host.Open()
printfn "Host is open."
Console.ReadKey(true) |> ignore
host.Close()