#light
#r "System.ServiceModel"
open System
open System.ServiceModel
open System.ServiceModel.Description


[<ServiceContract>]
type MyService() =
    [<OperationContract>]
    member this.MyMethod() =
        printfn "MyService.MyMethod()"


let uri = new Uri("net.tcp://localhost")
let wsBinding = new WSHttpBinding()
let tcpBinding = new NetTcpBinding()
let host = new ServiceHost(typeof<MyService>, [| uri |])

// An Endpoint is comprised of three things:
//   Address
//   Binding
//   Contract
host.AddServiceEndpoint(typeof<MyService>, wsBinding, "http://localhost:8000/MyService")
host.AddServiceEndpoint(typeof<MyService>, tcpBinding, "net.tcp://localhost:8001/MyService")
host.AddServiceEndpoint(typeof<MyService>, tcpBinding, "MyService", new Uri("net.tcp://localhost:8002"))

host.Open()
printfn "Service opened at following URI's:"
host.Description.Endpoints
|> Seq.iter (fun ep -> printfn "  %A" ep.Address.Uri)
host.Close()
