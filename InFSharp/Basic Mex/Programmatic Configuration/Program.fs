#light
open System.ServiceModel
open System.ServiceModel.Description

[<ServiceContract(ConfigurationName = "IMyContract")>]
type IMyContract =
    [<OperationContract>]
    abstract MyOperation : unit -> string

type MyService() =
    interface IMyContract with
        member this.MyOperation() = "MyResult"
        
let host = new ServiceHost(typeof<MyService>, [| new System.Uri("http://localhost") |])
// Not relevant to this example but necessary to get it to run
host.AddServiceEndpoint(typeof<IMyContract>, new WSHttpBinding(), "MyService") |> ignore

match host.Description.Behaviors.Find<ServiceMetadataBehavior>() with
| null ->
    let smb = new ServiceMetadataBehavior(HttpGetEnabled = true)
    host.Description.Behaviors.Add(smb)
| _    -> ()
        
host.Open()

let path = System.Environment.GetEnvironmentVariable("ProgramFiles")
System.Diagnostics.Process.Start(path + @"\internet explorer\iexplore.exe", "http://localhost") |> ignore
System.Console.ReadKey(true) |> ignore

host.Close()