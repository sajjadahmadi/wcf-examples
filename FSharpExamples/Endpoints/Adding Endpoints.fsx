#light
#r "System.ServiceModel"
#r "../System.ServiceModel.FSharp.dll"
open System
open System.ServiceModel
open System.ServiceModel.Description
open System.ServiceModel.FSharp.Uri
open System.ServiceModel.FSharp.ServiceHost


[<ServiceContract>]
type MyService() =
    [<OperationContract>]
    member this.MyMethod() =
        printfn "MyService.MyMethod()"


let host = create<MyService> None

let wsBinding = new WSHttpBinding()
let tcpBinding = new NetTcpBinding()

// An Endpoint is comprised of three things:
//   Address
//   Binding
//   Contract
host.AddServiceEndpoint(typeof<MyService>, wsBinding, "http://localhost:8000/MyService")
host.AddServiceEndpoint(typeof<MyService>, tcpBinding, "net.tcp://localhost:8001/MyService")
host.AddServiceEndpoint(typeof<MyService>, tcpBinding, "MyService", uri "net.tcp://localhost:8002")

opn host
printfn "Service opened at following URI's:"
listenUris host |> Seq.iter (fun u -> printfn "  %A" u)
Console.ReadKey(true) |> ignore
close host
