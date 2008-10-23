#light
open System
open System.ServiceModel
open System.Diagnostics

[<ServiceContract(ConfigurationName = "IMyContract")>]
type IMyContract =
    [<OperationContract>]
    abstract MyOperation : unit -> string

type MyService() =
    interface IMyContract with
        member this.MyOperation() = "MyResult"

let uris: Uri[] = [| |]
let host = new ServiceHost(typeof<MyService>, uris)
host.Open()

Debug.Assert(host.State = CommunicationState.Opened)
Debug.Assert(host.Description.Endpoints.Count = 6)
printfn "Service is up!"
Console.ReadKey(true) |> ignore