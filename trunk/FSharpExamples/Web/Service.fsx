// Adapted from http://msdn.microsoft.com/en-us/library/bb412178.aspx
#r "System.ServiceModel"
#r "System.ServiceModel.Web"
open System
open System.ServiceModel
open System.ServiceModel.Description
open System.ServiceModel.Web


[<ServiceContract>]
type IService =
    [<OperationContract>]
    [<WebGet>]
    abstract EchoWithGet : s : string -> string

    [<OperationContract>]
    [<WebInvoke>] // By default WebInvoke maps POST calls to the operation
    abstract EchoWithPost : s : string -> string


type Service() =
    interface IService with
        member this.EchoWithGet(s) =
            sprintf "GET You said %s" s
        
        member this.EchoWithPost(s) =
            sprintf "POST You said %s" s
        

let private uri = new Uri("http://localhost:8000")
let private host = new WebServiceHost(typeof<Service>, uri)

let private disableHelpPage (host : ServiceHost) =
    let b = host.Description.Behaviors.Find<ServiceDebugBehavior>()
    match b with
    | null -> ()
    | _    -> b.HttpHelpPageEnabled <- false

let start() =
    host.AddServiceEndpoint(typeof<IService>, new WebHttpBinding(), "") |> ignore
    disableHelpPage host
    host.Open()
    printfn "Service is running"
    printfn "Visit http://localhost:8000/EchoWithGet?s=HttpMessage for example"

let close() =
    host.Close()
