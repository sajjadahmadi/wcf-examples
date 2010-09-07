#r "System.ServiceModel"
open System
open System.IO
open System.Text
open System.ServiceModel

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
            newStream "Stream Reply: This string was read from a Stream."
        
        member this.StreamRequest(stream) =
            printfn "%s" (read stream)
        
        member this.OneWayStream(stream) =
            printfn "%s" (read stream)
            
            
let host = new ServiceHost(typeof<MyService>, new Uri("http://localhost"))
host.Open()

let proxy = ChannelFactory<IMyContract>.CreateChannel(host.Description.Endpoints.[0].Binding, host.Description.Endpoints.[0].Address)

proxy.StreamRequest(newStream "Stream Request: This string was read from a Stream.")

let response = proxy.StreamReply()
printfn "%s" (read response)
// Client is always responsible for closing reply streams
response.Close()

proxy.OneWayStream(newStream "One-Way Stream: This string was read from a Stream.")

(proxy :?> ICommunicationObject).Close()
host.Close()
