#r "System.ServiceModel"
#r "System.Runtime.Serialization"
#r @"..\..\bin\Mcts70_503.dll"
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

example<Service, IContract>
    (fun host _ ->
        let factory = host.CreateChannelFactory()
        printf "Enter Windows user name: "
        let name = Console.ReadLine()
        printf "Enter Windows password: "
        let password = Console.ReadLine()
        Console.Clear()
        
        factory.Credentials.Windows.ClientCredential <- new NetworkCredential(name, password)
        factory.Credentials.Windows.AllowedImpersonationLevel <- TokenImpersonationLevel.Anonymous
        let proxy = factory.CreateChannel()
        
        try
            proxy.ImpersonatingOperation()
        with ex -> 
            printfn "%s" ex.Message
            (proxy :?> ICommunicationObject).Abort())
