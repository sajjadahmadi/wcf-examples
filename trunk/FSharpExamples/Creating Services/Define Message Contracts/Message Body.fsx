#r "System.Runtime.Serialization"
#r "System.ServiceModel"
open System.IO
open System.Xml
open System.ServiceModel.Channels
open System.Runtime.Serialization


[<DataContract>]
type Body =
    { [<DataMember>] mutable Content : string }


let body = { Content = "Content" }

let v = MessageVersion.Soap12
let msg = Message.CreateMessage(v, "action", body)

let b = msg.GetBody<Body>()
printfn "Message"
printfn "  Status: %A" msg.State
printfn "  Body: %A" b
