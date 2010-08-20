#r "System.ServiceModel"
#r "System.Runtime.Serialization"
open System
open System.Collections.Generic
open System.ServiceModel
open System.ServiceModel.Description


[<ServiceContract(SessionMode = SessionMode.Required)>]
type IMyContract =
    [<OperationContract>]
    abstract IncrementCounter : unit -> unit


[<ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)>]
type MyService() =
    let counters = new Dictionary<string, int>()
    
    interface IMyContract with
        member this.IncrementCounter() =
            let sid = OperationContext.Current.SessionId
            if not (counters.ContainsKey(sid))
                then counters.Add(sid, 0)
            
            let c = counters.[sid] + 1
            counters.[sid] <- c
            printfn "Session %s; Counter = %d" sid c


let uris = [| new Uri("http://localhost"); new Uri("net.tcp://localhost") |]
let binding1, binding2 = new WSHttpBinding(), new NetTcpBinding()
let host = new ServiceHost(typeof<MyService>, uris)
host.AddServiceEndpoint(typeof<IMyContract>, binding1, "")
host.AddServiceEndpoint(typeof<IMyContract>, binding2, "")
let b = host.Description.Behaviors.Find<ServiceDebugBehavior>()
b.IncludeExceptionDetailInFaults <- true
host.Open()

let proxy1 = ChannelFactory<IMyContract>.CreateChannel(binding1, new EndpointAddress(string uris.[0]))
let proxy2 = ChannelFactory<IMyContract>.CreateChannel(binding2, new EndpointAddress(string uris.[1]))

proxy1.IncrementCounter()
proxy1.IncrementCounter()
proxy2.IncrementCounter()
proxy2.IncrementCounter()

(proxy1 :?> ICommunicationObject).Close()
(proxy2 :?> ICommunicationObject).Close()
host.Close()


