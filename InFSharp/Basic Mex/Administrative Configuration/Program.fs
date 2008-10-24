#light
open System.ServiceModel

[<ServiceContract(ConfigurationName = "IMyContract")>]
type IMyContract =
    [<OperationContract>]
    abstract MyOperation : unit -> string

type MyService() =
    interface IMyContract with
        member this.MyOperation() = "MyResult"

let uris: System.Uri[] = [| |]
let host = new ServiceHost(typeof<MyService>, uris)

host.Open()

let path = System.Environment.GetEnvironmentVariable("ProgramFiles")
System.Diagnostics.Process.Start(path + @"\internet explorer\iexplore.exe", "http://localhost") |> ignore
System.Console.ReadKey(true) |> ignore

host.Close()
