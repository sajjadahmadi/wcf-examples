#light
#r "System.ServiceModel"
#r "System.Runtime.Serialization"
open System
open System.ServiceModel
open System.ServiceModel.Channels
open System.Threading


type MyResource() =
    let context = new SynchronizationContext()
    
    member this.DoWork() =
        printfn "MyResource.DoWork(): %d" Thread.CurrentThread.ManagedThreadId
    
    member this.SynchronizationContext = context


[<ServiceContract>]
type IMyContract =
    [<OperationContract>]
    abstract MySynchronizedMethod : unit -> unit
    
    [<OperationContract>]
    abstract MyUnsynchronizedMethod : unit -> unit


[<ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)>]
type MyService(resource : MyResource) =
    interface IMyContract with
        member this.MySynchronizedMethod() =
            printfn "MyService.MySynchronizedMethod(): %d" Thread.CurrentThread.ManagedThreadId
            let context = resource.SynchronizationContext
            context.Send((fun state ->
                resource.DoWork()), null)

        member this.MyUnsynchronizedMethod() =
            printfn "MyService.MyUnsynchronizedMethod(): %d" Thread.CurrentThread.ManagedThreadId


printfn "main(): %d" Thread.CurrentThread.ManagedThreadId
let resource = new MyResource()
resource.DoWork()

let uri = new Uri("net.tcp://localhost")
let binding = new NetTcpBinding()
let service = new MyService(resource)
let host = new ServiceHost(service, [| uri |])
host.AddServiceEndpoint(typeof<IMyContract>, binding, "")
host.Open()

let proxy = ChannelFactory<IMyContract>.CreateChannel(binding, new EndpointAddress("net.tcp://localhost"))
proxy.MySynchronizedMethod()
proxy.MyUnsynchronizedMethod()

(proxy :?> ICommunicationObject).Close()
host.Close()
