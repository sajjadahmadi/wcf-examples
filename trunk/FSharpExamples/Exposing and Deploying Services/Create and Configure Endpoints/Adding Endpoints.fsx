#r "System.ServiceModel"
#r @"..\..\bin\Mcts70_503.dll"
open System
open System.ServiceModel
open System.ServiceModel.Description
Console.Clear()


[<ServiceContract>]
type MyService() =
    [<OperationContract>]
    member this.MyMethod() =
        printfn "MyService.MyMethod()"


let wsBinding = new WSHttpBinding()
let tcpBinding = new NetTcpBinding()
let host = new ExampleHost<MyService, MyService>()

// An Endpoint is comprised of three things:
//   Address
//   Binding
//   Contract

// Add by specifying the contract, the binding, and the absolute address
host.AddServiceEndpoint(typeof<MyService>, wsBinding, "http://localhost:8000/MyService")

// Add by specifying the contract, the binding, and the relative address
host.AddServiceEndpoint(typeof<MyService>, tcpBinding, "MyService")

// Add by specifying the contract, the binding, the relative address, and the listen URI
host.AddServiceEndpoint(typeof<MyService>, tcpBinding, "MyService2", new Uri("net.tcp://localhost:8002"))

host.Open()
printfn "Service opened at following URI's:"
host.Description.Endpoints
|> Seq.iter (fun ep -> printfn "  %A" ep.Address.Uri)
host.Close()
