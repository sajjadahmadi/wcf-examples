#light
#r "System.ServiceModel"
#r "System.Runtime.Serialization"
open System
open System.IO
open System.Text
open System.ServiceModel
open System.ServiceModel.Channels

let newStream (s: string) =
    let data = Encoding.UTF8.GetBytes(s)
    new MemoryStream(data) :> Stream

let read (s: Stream) =
    let reader = new StreamReader(s)
    reader.ReadToEnd()


[<ServiceContract>]
type IMyContract =
    [<OperationContract>]
    abstract StreamReply : unit -> Stream
    
    [<OperationContract>]
    abstract StreamRequest : Stream -> unit
    
    [<OperationContract(IsOneWay = true)>]
    abstract OneWayStream : Stream -> unit
    
    
type MyService() =
    interface IMyContract with
        member this.StreamReply() =
            newStream "MyService.IMyContract()"
        
        member this.StreamRequest(stream) =
            printfn "%s" (read stream)
        
        member this.OneWayStream(stream) =
            printfn "%s" (read stream)
            
            
let uri = new Uri("http://localhost")
let binding = new BasicHttpBinding(TransferMode = TransferMode.Streamed)
let host = new ServiceHost(typeof<MyService>, [| uri |])
host.AddServiceEndpoint(typeof<IMyContract>, binding, "")
host.Open()

let proxy = ChannelFactory<IMyContract>.CreateChannel(binding, new EndpointAddress(string uri))
proxy.StreamRequest(newStream "proxy.StreamRequest()")

let response = proxy.StreamReply()
printfn "%s" (read response)
// Client is always responsible for closing reply streams
response.Close()

proxy.OneWayStream(newStream "proxy.OneWayStream()")

Threading.Thread.Sleep(100)
printfn "==Press any key to END=="
Console.ReadKey(true)

(proxy :?> ICommunicationObject).Close()
host.Close()
