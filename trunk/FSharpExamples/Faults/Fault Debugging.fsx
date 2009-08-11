#r "System.ServiceModel"
#r "System.Runtime.Serialization"
open System
open System.ServiceModel


[<ServiceContract>]
type IMyContract =
    [<OperationContract>]
    abstract MethodWithError : unit -> unit


type MyService() =
    interface IMyContract with
        member this.MethodWithError() =
            let ex = new InvalidOperationException("Some error")
            let detail = new ExceptionDetail(ex)
            let fault = new FaultException<ExceptionDetail>(detail, ex.Message)
            raise fault


let uri = new Uri("net.tcp://localhost")
let binding = new NetTcpBinding()
let host = new ServiceHost(typeof<MyService>, [| uri |])
host.AddServiceEndpoint(typeof<IMyContract>, binding, "")
host.Open()

let proxy = ChannelFactory<IMyContract>.CreateChannel(binding, new EndpointAddress(string uri))
try
    proxy.MethodWithError()
with
    | :? FaultException<ExceptionDetail> as ex ->
        printfn "%s: %s" ex.Detail.Type ex.Detail.Message

(proxy :?> ICommunicationObject).Close()
host.Close()