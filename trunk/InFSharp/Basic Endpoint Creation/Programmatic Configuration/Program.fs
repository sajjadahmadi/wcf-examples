#light
open System
open System.ServiceModel
open System.Diagnostics

[<ServiceContract>]
type IMyContract =
    [<OperationContract>]
    abstract MyOperation : unit -> string

type MyService() =
    interface IMyContract with
        member this.MyOperation() = "MyResult"
        
let baseAddress = [| new Uri("http://localhost:8000") |]
let host = new ServiceHost(typeof<MyService>, baseAddress)
let wsBinding = new WSHttpBinding()
let tcpBinding = new NetTcpBinding()

// Base address only
host.AddServiceEndpoint(typeof<IMyContract>, wsBinding, "") |> ignore

// Relative to base address
host.AddServiceEndpoint(typeof<IMyContract>, wsBinding, "MyService") |> ignore

// Absolute address
host.AddServiceEndpoint(typeof<IMyContract>, wsBinding, "http://localhost:8001/MyService") |> ignore

// Can add multiple endpoints of the same type as long as they differ by URL
host.AddServiceEndpoint(typeof<IMyContract>, wsBinding, "http://localhost:8002/MyService") |> ignore
host.AddServiceEndpoint(typeof<IMyContract>, tcpBinding, "net.tcp://localhost:8003/MyService") |> ignore
host.AddServiceEndpoint(typeof<IMyContract>, tcpBinding, "net.tcp://localhost:8004/MyService") |> ignore

host.Open()
Debug.Assert(host.State = CommunicationState.Opened)
Debug.Assert(host.Description.Endpoints.Count = 6)
printfn "Service is up!"
Console.ReadKey(true) |> ignore