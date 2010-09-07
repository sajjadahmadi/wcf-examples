#r "System.ServiceModel"
#r "System.Runtime.Serialization"
open System
open System.Diagnostics
open System.ServiceModel
open System.ServiceModel.Channels


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


type OrderManager() =
    let mutable total = 0
    
    interface IOrderManager with
        member this.SetCustomerId(id) = printfn "Setting customer id to %d..." id
        
        member this.AddItem(item) = printfn "Adding item %d..." item; total <- total + item
        
        member this.GetTotal() = decimal total
        
        member this.ProcessOrders() = printfn "Processing orders..."; true


let host = new ServiceHost(typeof<OrderManager>, new Uri("net.tcp://localhost"))
host.Open()

let proxy = ChannelFactory<IOrderManager>.CreateChannel(host.Description.Endpoints.[0].Binding, host.Description.Endpoints.[0].Address)
// Calling a non-initiating operation first will error
try
    proxy.AddItem(1)
with ex -> printfn "%s\n\n" ex.Message

proxy.SetCustomerId(1)
proxy.AddItem(4)
proxy.AddItem(5)
proxy.AddItem(6)
printfn "Total = %0.2f" <| proxy.GetTotal()
proxy.ProcessOrders()

// Session has been closed; no more messages may be sent
try
    proxy.SetCustomerId(1)
with ex -> printfn "\n\n%s\n" ex.Message

(proxy :?> ICommunicationObject).Close()
host.Close()
