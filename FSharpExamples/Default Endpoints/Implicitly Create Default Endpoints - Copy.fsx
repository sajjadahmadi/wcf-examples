#r "System.ServiceModel"
open System
open System.ServiceModel


[<ServiceContract>]
type IServiceContract =
    [<OperationContract>]
    abstract AnOperation : string -> unit


type ServiceClass() =
    interface IServiceContract with
        member this.AnOperation(arg) =
            printfn "AnOperation(%s)" arg


let uris = [| new Uri("http://localhost:8080"); new Uri("net.tcp://localhost:8081") |]
let host = new ServiceHost(typeof<ServiceClass>, uris)
host.Open()

for endpoint in host.Description.Endpoints do
    printfn "A: %O, B: %O, C: %O" endpoint.Address endpoint.Binding.Name endpoint.Contract.Name

host.Close()
