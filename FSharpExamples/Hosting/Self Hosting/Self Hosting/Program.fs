#light
open System
open System.Diagnostics
open System.ServiceModel

[<ServiceContract>]
type IMyContract =
    [<OperationContract>]
    abstract MyOperation : unit -> string

type MyService() =
    interface IMyContract with
        member this.MyOperation() = "MyResult"


let uris: Uri[] = [| |]
let host = new ServiceHost(typeof<MyService>, uris)
host.Open()

let iePath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Internet Explorer\\IEXPLORE.EXE")
Process.Start(iePath, "http://localhost:8000") |> ignore
Console.ReadKey(true) |> ignore
host.Close()
