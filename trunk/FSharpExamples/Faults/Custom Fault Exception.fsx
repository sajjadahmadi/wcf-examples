#light
#r "System.ServiceModel"
#r "System.Runtime.Serialization"
open System
open System.ServiceModel
open System.ServiceModel.Channels
open System.Runtime.Serialization


type MyCustomFault(msg : string) =
    inherit FaultException(msg)


[<ServiceContract>]
type IMyContract =
    [<OperationContract>]
    abstract MyMethod : unit -> unit
    

type MyService() =
    interface IMyContract with
        member this.MyMethod() =
            raise (new MyCustomFault("Error Message"))
            

let uri = new Uri("net.tcp://localhost")
let binding = new NetTcpBinding()
let host = new ServiceHost(typeof<MyService>, [| uri |])
host.AddServiceEndpoint(typeof<IMyContract>, binding, "")
host.Open()

let proxy = ChannelFactory<IMyContract>.CreateChannel(binding, new EndpointAddress(string uri))

try
    proxy.MyMethod()
with :? FaultException as ex -> 
    printfn "%A: %s" (ex.GetType()) ex.Message
    printfn "  Action: %s" ex.Action
    printfn "  Code:   %s" ex.Code.Name
    printfn "  Reason: %A" ex.Reason

printfn "Host State: %A" host.State
printfn "Client State: %A" (proxy :?> ICommunicationObject).State
host.Close()

