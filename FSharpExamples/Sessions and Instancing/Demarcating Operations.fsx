#r "System.ServiceModel"
#r "System.Runtime.Serialization"
open System
open System.ServiceModel
Console.Clear()


[<ServiceContract(SessionMode = SessionMode.Required)>]
type IMyContract =
    [<OperationContract(IsOneWay = true)>]
    abstract OperationA : unit -> unit
    
    [<OperationContract(IsOneWay = true)>]
    abstract OperationB : unit -> unit
    
    [<OperationContract(IsOneWay = true, IsInitiating = false)>]
    abstract OperationC : unit -> unit
    
    [<OperationContract(IsOneWay = true, IsInitiating = false, IsTerminating = true)>]
    abstract OperationD : unit -> unit
    
    [<OperationContract(IsOneWay = true, IsInitiating = false, IsTerminating = true)>]
    abstract OperationE : unit -> unit
    

type MyService() =
    interface IMyContract with
        member this.OperationA() = printfn "OperationA()"
        
        member this.OperationB() = printfn "OperationB()"
        
        member this.OperationC() = printfn "OperationC()"
        
        member this.OperationD() = printfn "OperationD()"
        
        member this.OperationE() = printfn "OperationE()"


let uri = new Uri("net.tcp://localhost")
let binding = new NetTcpBinding()
let host = new ServiceHost(typeof<MyService>, uri)
host.AddServiceEndpoint(typeof<IMyContract>, binding, "")
host.Open()

let mutable proxy = ChannelFactory<IMyContract>.CreateChannel(binding, new EndpointAddress(string uri))
let reset() =
    (proxy :?> ICommunicationObject).Abort()
    proxy <- ChannelFactory<IMyContract>.CreateChannel(binding, new EndpointAddress(string uri))

try
    printfn "=============================\nAttempted order: A, B, C, D, E"
    proxy.OperationA()
    proxy.OperationB()
    proxy.OperationC()
    proxy.OperationD()
    proxy.OperationE()
with ex -> printfn "Error: %s\n" ex.Message

try
    printfn "=============================\nAttempted order: A, C, D"
    reset()
    proxy.OperationA()
    proxy.OperationC()
    proxy.OperationD()
with ex -> printfn "Error: %s\n" ex.Message

try
    printfn "=============================\nAttempted order: A, D, C"
    reset()
    proxy.OperationA()
    proxy.OperationD()
    proxy.OperationC()
with ex -> printfn "Error: %s\n" ex.Message

try
    printfn "=============================\nAttempted order: A, E"
    reset()
    proxy.OperationA()
    proxy.OperationE()
with ex -> printfn "Error: %s\n" ex.Message

try
    printfn "=============================\nAttempted order: B, A, C, D, E"
    reset()
    proxy.OperationB()
    proxy.OperationA()
    proxy.OperationC()
    proxy.OperationD()
    proxy.OperationE()
with ex -> printfn "Error: %s\n" ex.Message

try
    printfn "=============================\nAttempted order: B, C, D"
    reset()
    proxy.OperationB()
    proxy.OperationC()
    proxy.OperationD()
with ex -> printfn "Error: %s\n" ex.Message

try
    printfn "=============================\nAttempted order: C, A, B, D"
    reset()
    proxy.OperationC()
    proxy.OperationA()
    proxy.OperationB()
    proxy.OperationD()
with ex -> printfn "Error: %s\n" ex.Message

try
    printfn "=============================\nAttempted order: C, E"
    reset()
    proxy.OperationC()
    proxy.OperationE()
with ex -> printfn "Error: %s\n" ex.Message

(proxy :?> ICommunicationObject).Abort()
host.Close()
