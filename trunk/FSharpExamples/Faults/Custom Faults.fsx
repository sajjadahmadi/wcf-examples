#r "System.ServiceModel"
#r "System.Runtime.Serialization"
open System
open System.ServiceModel
open System.ServiceModel.Channels
open System.Runtime.Serialization


[<DataContract>]
type MyCustomFault =
    { [<DataMember>] mutable Message : string }


[<ServiceContract>]
type IMyContract =
    [<OperationContract>]
    [<FaultContract(typeof<MyCustomFault>)>]
    abstract MyMethod : unit -> unit
    

type MyService() =
    interface IMyContract with
        member this.MyMethod() =
            raise (new FaultException<MyCustomFault>({ Message = "Error Message" }))
            

let uri = new Uri("net.tcp://localhost")
let binding = new NetTcpBinding()
let host = new ServiceHost(typeof<MyService>, [| uri |])
host.AddServiceEndpoint(typeof<IMyContract>, binding, "")
host.Open()

let proxy = ChannelFactory<IMyContract>.CreateChannel(binding, new EndpointAddress(string uri))

try
    proxy.MyMethod()
with :? FaultException<MyCustomFault> as ex -> 
    printfn "%A: %s" (ex.GetType()) ex.Detail.Message
    printfn "  Action: %s" ex.Action
    printfn "  Code:   %s" ex.Code.Name
    printfn "  Reason: %A" ex.Reason

(proxy :?> ICommunicationObject).Close()
host.Close()

