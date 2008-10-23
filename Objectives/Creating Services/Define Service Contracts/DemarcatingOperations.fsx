#light
#r "System.ServiceModel"
#r "System.Runtime.Serialization"
#load "../../ref/InProcHost.fsx"
open Mcts_70_503
open System
open System.Diagnostics
open System.ServiceModel

[<ServiceContract(SessionMode = SessionMode.Required)>]
type IOrderManager =
    [<OperationContract>]
    abstract SetCustomerId : int -> unit
    
    [<OperationContract(IsInitiating = false)>]
    abstract AddItem : int -> unit
    
    [<OperationContract(IsInitiating = false)>]
    abstract GetTotal : unit -> decimal
    
    [<OperationContract(IsInitiating = false, IsTerminating = true)>]
    abstract ProcessOrders : unit -> bool

[<ServiceBehavior>]
type OrderManager() =
    let mutable total = 0
    
    interface IOrderManager with
        member this.SetCustomerId(id) = printfn "Setting customer id to %d..." id
        
        member this.AddItem(item) = printfn "Adding item %d..." item; total <- total + item
        
        member this.GetTotal() = decimal total
        
        member this.ProcessOrders() = printfn "Processing orders..."; true

let host = new InProcHost<OrderManager>()
host.AddEndPoint<IOrderManager>()
host.Open()

let proxy = host.CreateProxy<IOrderManager>()
// Calling a non-initiaing operation first will error
try
    proxy.AddItem(1)
with ex -> printfn "%s\n\n" ex.Message

proxy.SetCustomerId(1)
proxy.AddItem(4)
proxy.AddItem(5)
proxy.AddItem(6)
printfn "Total = %A" (proxy.GetTotal())
proxy.ProcessOrders()

// Session has been closed; no more messages may be sent
try
    proxy.SetCustomerId(1)
with ex -> printfn "\n\n%s\n" ex.Message

host.CloseProxy(proxy)
host.Close()
