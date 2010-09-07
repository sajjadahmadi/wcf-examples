module Service
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
            printfn "  In EchoWithGet(s)"
            sprintf "You said %s" s
        
        member this.EchoWithPost(s) = 
            printfn "  In EchoWithPost(s)" 
            (this :> IService).EchoWithGet(s)
        

let private uri = new Uri("http://localhost:8000")
let private host = new WebServiceHost(typeof<Service>, uri)
host.AddServiceEndpoint(typeof<IService>, new WebHttpBinding(), "") |> ignore
host.Description.Endpoints.[0].Behaviors.Add(new WebHttpBehavior(HelpEnabled=true))
host.Open()

printfn "Service is running"
printfn "Visit http://localhost:8000/EchoWithGet?s=HttpMessage for example\n"
printfn "Or visit http://localhost:8000/help for the Help page"

let close() =
    host.Close()
