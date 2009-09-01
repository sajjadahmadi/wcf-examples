#r "System.ServiceModel"
#r "System.Runtime.Serialization"
#r @"..\..\bin\Mcts70_503.dll"
open System
open System.Net
open System.ServiceModel
open System.Security.Principal
Console.Clear()

let whoami() =
    OperationContext.Current.ServiceSecurityContext.WindowsIdentity.Name


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

example<Service, IContract>(fun _ proxy ->
    proxy.ImpersonatingOperation()
    proxy.NonimpersonatingOperation()) // No difference between calls, is there a way to demonstrate this?
