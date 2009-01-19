#light
#r "System.Runtime.Serialization"
#r "System.ServiceModel"
open System.IO
open System.Xml
open System.ServiceModel.Channels
open System.Runtime.Serialization

let ns = "http://schemas.mynamespace.org"


[<DataContract(Name = "MyHeader", Namespace = "http://schemas.mynamespace.org")>]
type MyHeader =
    { [<DataMember>] mutable Content : string }


let myhdr = { Content = "Content" }

let v = MessageVersion.Soap12
let msg = Message.CreateMessage(v, "action", "")
MessageHeader.CreateHeader("MyHeader", ns, myhdr)
|> msg.Headers.Add

let result = msg.Headers.GetHeader<MyHeader>("MyHeader", ns)
printfn "%A" result