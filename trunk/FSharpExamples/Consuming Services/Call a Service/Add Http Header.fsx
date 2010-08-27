#r "System.ServiceModel"
#r "System.Runtime.Serialization"
#r @"..\..\bin\Mcts70_503.dll"
open System
open System.Net
open System.ServiceModel
open System.ServiceModel.Channels
Console.Clear()

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
                printfn "%s: %s" h (hdrs.[h])


example2<MyService, IMyContract>
    (fun() -> 
        let binding = new BasicHttpBinding()
        new ExampleHost<MyService, IMyContract>(binding, "http://localhost:8000"))

    (fun _ proxy ->
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

        proxy.MyMethod())
