#r "System.ServiceModel"
#r "System.Runtime.Serialization"
open System
open System.Net
open System.ServiceModel
open System.ServiceModel.Channels

let httpPropName = HttpRequestMessageProperty.Name


[<ServiceContract>]
type IMyContract =
    [<OperationContract>]
    abstract MyMethod : unit -> unit


type MyService() =
    interface IMyContract with
        member this.MyMethod() =
            let httpProp = OperationContext.Current.IncomingMessageProperties.[httpPropName] :?> HttpRequestMessageProperty
            let hdrs = httpProp.Headers
            printfn "\nIncomingHeaders\n-----------------"
            for h in hdrs do
                printfn "%s: %s" h hdrs.[h]


let host = new ServiceHost(typeof<MyService>, new Uri("http://localhost:8000"))
host.Open()

let proxy = ChannelFactory<IMyContract>.CreateChannel(host.Description.Endpoints.[0].Binding, host.Description.Endpoints.[0].Address)
let scope = new OperationContextScope(proxy :?> IContextChannel)

let outMsgProps = OperationContext.Current.OutgoingMessageProperties
let httpProp =
    if outMsgProps.ContainsKey(httpPropName) then
        outMsgProps.[httpPropName] :?> HttpRequestMessageProperty
    else
        let p = new HttpRequestMessageProperty()
        outMsgProps.Add(httpPropName, p)
        p

httpProp.Headers.Add(HttpRequestHeader.UserAgent, "F#")

proxy.MyMethod()

host.Close()