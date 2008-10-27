#light
#r "System.ServiceModel"
#r "System.Runtime.Serialization"
#load "InProcHost.fsx"
open Mcts_70_503
open System
open System.IO
open System.Text
open System.ServiceModel

let newStream (s: string) =
    new MemoryStream(Encoding.UTF8.GetBytes(s)) :> Stream

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
            
let host = new InProcHost<MyService>()
let binding = new BasicHttpBinding(TransferMode = TransferMode.Streamed)
host.AddEndpoint<IMyContract>(binding)
host.Open()

let proxy = host.CreateProxy<IMyContract>()
proxy.StreamRequest(newStream "proxy.StreamRequest()")

let response = proxy.StreamReply()
printfn "%s" (read response)

proxy.OneWayStream(newStream "proxy.OneWayStream()")

Threading.Thread.Sleep(100)
printfn "==Press any key to END=="
Console.ReadKey(true)

host.CloseProxy(proxy)
host.Close()
