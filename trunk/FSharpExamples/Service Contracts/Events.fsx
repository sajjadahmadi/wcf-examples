#light
#r "System.ServiceModel"
#r "System.Runtime.Serialization"
open System
open System.Collections.Generic
open System.ServiceModel
open System.ServiceModel.Channels

type EventType =
    | Event1    = 1
    | Event2    = 2
    | Event3    = 4
    | AllEvents = 7


// Event operations should be * -> unit
type IMyEvents =
    [<OperationContract(IsOneWay = true)>]
    abstract OnEvent1 : unit -> unit
    
    [<OperationContract(IsOneWay = true)>]
    abstract OnEvent2 : int -> unit
    
    [<OperationContract(IsOneWay = true)>]
    abstract OnEvent3 : int * string -> unit


[<ServiceContract(CallbackContract = typeof<IMyEvents>)>]
type IMyContract =
    [<OperationContract>]
    abstract DoSomething : unit -> unit
    
    [<OperationContract>]
    abstract Subscribe : EventType -> unit
    
    [<OperationContract>]
    abstract Unsubscribe : EventType -> unit


[<ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)>]
type MyPublisher() =
    let event1 = new Dictionary<IMyEvents, unit -> unit>()
    let event2 = new Dictionary<IMyEvents, int -> unit>()
    let event3 = new Dictionary<IMyEvents, int * string -> unit>()

    member this.FireEvent = function
        | EventType.Event1    -> event1 |> Seq.iter (fun f -> f.Value())
        | EventType.Event2    -> event2 |> Seq.iter (fun f -> f.Value(42))
        | EventType.Event3    -> event3 |> Seq.iter (fun f -> f.Value(42, "Hello"))
        | EventType.AllEvents ->
            this.FireEvent EventType.Event1
            this.FireEvent EventType.Event2
            this.FireEvent EventType.Event3
        | _                   -> failwith "invalid EventType"
            
    interface IMyContract with
        member this.DoSomething() = ()
        
        member this.Subscribe(eventType) =
            let subscriber = OperationContext.Current.GetCallbackChannel<IMyEvents>()
            match eventType with
            | EventType.Event1    -> event1.Add(subscriber, subscriber.OnEvent1)
            | EventType.Event2    -> event2.Add(subscriber, subscriber.OnEvent2)
            | EventType.Event3    -> event3.Add(subscriber, subscriber.OnEvent3)
            | EventType.AllEvents ->
                event1.Add(subscriber, subscriber.OnEvent1)
                event2.Add(subscriber, subscriber.OnEvent2)
                event3.Add(subscriber, subscriber.OnEvent3)
            | _ -> failwith "invalid EventType"

        member this.Unsubscribe(eventType) =
            let subscriber = OperationContext.Current.GetCallbackChannel<IMyEvents>()
            match eventType with
            | EventType.Event1    -> event1.Remove subscriber |> ignore
            | EventType.Event2    -> event2.Remove subscriber |> ignore
            | EventType.Event3    -> event3.Remove subscriber |> ignore
            | EventType.AllEvents ->
                event1.Remove subscriber |> ignore
                event2.Remove subscriber |> ignore
                event3.Remove subscriber |> ignore
            | _ -> failwith "invalid EventType"
            
        
type MyContractClient(callbackInstance: obj, binding: Binding, remoteAddress: EndpointAddress) =
    inherit DuplexClientBase<IMyContract>(callbackInstance, binding, remoteAddress)


type MyEventClient() =
    interface IMyEvents with
        member this.OnEvent1() = printfn "MyEventClient.OnEvent1()"
        
        member this.OnEvent2(n) = printfn "MyEventClient.OnEvent2(%d)" n
        
        member this.OnEvent3(n, s) = printfn "MyEventClient.OnEvent3(%d, %s)" n s



let uri = new Uri("net.tcp://localhost")
let binding = new NetTcpBinding()
let publisher = new MyPublisher()
let host = new ServiceHost(publisher, uri)
host.AddServiceEndpoint(typeof<IMyContract>, binding, "")
host.Open()

let callback = new MyEventClient()
let client = new MyContractClient(callback, binding, new EndpointAddress(string uri))
let proxy = client.ChannelFactory.CreateChannel()

// Subscribe to events
proxy.Subscribe(EventType.AllEvents)

// Fire events
publisher.FireEvent(EventType.AllEvents)

Threading.Thread.Sleep(100)
printfn "==Press any key to CONTINUE=="
Console.ReadKey(true)

// Unsubscribe from a single event
proxy.Unsubscribe(EventType.Event1)

// Fire events
publisher.FireEvent(EventType.AllEvents)

Threading.Thread.Sleep(100)
printfn "==Press any key to END=="
Console.ReadKey(true)

(proxy :?> ICommunicationObject).Close()
host.Close()
