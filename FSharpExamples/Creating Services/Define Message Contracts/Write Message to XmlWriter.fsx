#r "System.Runtime.Serialization"
#r "System.ServiceModel"
open System.IO
open System.Xml
open System.ServiceModel.Channels
System.Console.Clear()

let body = "Body"

let stream = new MemoryStream()
let xmlWriter = XmlDictionaryWriter.CreateTextWriter(stream)

let v = MessageVersion.Soap12
let msg = Message.CreateMessage(v, "action", body)
msg.WriteMessage(xmlWriter)

xmlWriter.Flush()
stream.Position <- 0L
let reader = new StreamReader(stream)
printfn "Message"
printfn "  Status: %A" msg.State
printfn "  Body: %s" (reader.ReadToEnd())
