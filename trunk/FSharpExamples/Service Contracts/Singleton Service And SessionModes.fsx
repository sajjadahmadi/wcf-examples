#r "System.ServiceModel"
#r "System.Runtime.Serialization"
open System
open System.ServiceModel


[<ServiceContract(SessionMode = SessionMode.Required)>]
type IMyContract =
    [<OperationContract>]
    abstract MyMethod : unit -> unit


[<ServiceContract(SessionMode = SessionMode.NotAllowed)>]
type IMyOtherContract =
    [<OperationContract>]
    abstract MyOtherMethod : unit -> unit
    

// Try the following to highlight the differences
//[<ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession)>]
[<ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)>]
type MySingleton() =
    let mutable counter = 0
    
    do printfn "MySingleton.MySingleton()"
    
    interface IMyContract with
        member this.MyMethod() =
            counter <- counter + 1
            printfn "Counter = %d" counter
    
    interface IMyOtherContract with
        member this.MyOtherMethod() =
            counter <- counter + 1
            printfn "Counter = %d" counter
    
    interface IDisposable with
        member this.Dispose() =
            printfn "MySingleton.Dispose()"


let uri = new Uri("http://localhost")
let wsBinding = new WSHttpBinding(SecurityMode.None, true)
let httpBinding = new BasicHttpBinding()
let host = new ServiceHost(typeof<MySingleton>, [| uri |])
host.AddServiceEndpoint(typeof<IMyContract>, wsBinding, "")
host.AddServiceEndpoint(typeof<IMyOtherContract>, httpBinding, "basic")
host.Open()

let proxy1 = ChannelFactory<IMyContract>.CreateChannel(wsBinding, new EndpointAddress(string uri))
proxy1.MyMethod() // 1
proxy1.MyMethod() // 2
(proxy1 :?> ICommunicationObject).Close()

let proxy2 = ChannelFactory<IMyOtherContract>.CreateChannel(httpBinding, new EndpointAddress(string uri + "/basic"))
proxy2.MyOtherMethod() // 3 or 1
proxy2.MyOtherMethod() // 4 or 1
(proxy2 :?> ICommunicationObject).Close()
host.Close()
