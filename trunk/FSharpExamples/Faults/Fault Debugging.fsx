#light
#r "System.ServiceModel"
#r "System.Runtime.Serialization"
open System
open System.Runtime.Serialization
open System.ServiceModel
open System.ServiceModel.Channels


[<ServiceContract>]
type IMyContract =
    [<OperationContract>]
    abstract MethodWithError : unit -> unit


type MyService() =
    interface IMyContract with
        member this.MethodWithError() =
            try
                raise (new InvalidOperationException("Some error"))
            with ex ->
                let detail = new ExceptionDetail(ex)
                raise (new FaultException<ExceptionDetail>(detail, ex.Message))


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
        printfn "%s" ex.Detail.StackTrace

(proxy :?> ICommunicationObject).Close()
host.Close()