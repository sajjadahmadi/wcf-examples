#r "System.ServiceModel"
#r "System.Runtime.Serialization"
open System
open System.Net
open System.ServiceModel
open System.Security.Principal
Console.Clear()

let whoami() =
    WindowsIdentity.GetCurrent().Name


[<ServiceContract>]
type IContract =
    [<OperationContract>]
    abstract ImpersonatingOperation : unit -> unit
    
    [<OperationContract>]
    abstract NonimpersonatingOperation : unit -> unit


type Service() =
    interface IContract with
        [<OperationBehavior(Impersonation = ImpersonationOption.Required)>]
        member this.ImpersonatingOperation() =
            printfn "Impersonating %s" (whoami())
        
        member this.NonimpersonatingOperation() =
            printfn "No impersonation, running as %s" (whoami())


let host = new ServiceHost(typeof<Service>, new Uri("net.tcp://localhost:8081"))
host.Open()

let channelFactory = new ChannelFactory<IContract>(host.Description.Endpoints.[0])
printf "Enter Windows user name: "
let name = Console.ReadLine()
printf "Enter Windows password: "
let password = Console.ReadLine()
Console.Clear()

channelFactory.Credentials.Windows.ClientCredential <- new NetworkCredential(name, password)
let proxy = channelFactory.CreateChannel()

proxy.ImpersonatingOperation()
proxy.NonimpersonatingOperation()

(proxy :?> ICommunicationObject).Close()
host.Close()
