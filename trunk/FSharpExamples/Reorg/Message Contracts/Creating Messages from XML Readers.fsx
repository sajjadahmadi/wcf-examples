#r "System.ServiceModel"
#r "System.Runtime.Serialization"
open System
open System.IO
open System.Xml
open System.Runtime.Serialization
open System.ServiceModel
open System.ServiceModel.Channels


[<ServiceContract(Namespace = "")>]
type IMyService =
    [<OperationContract>]
    abstract GetData : unit -> Message


type MyService() =
    interface IMyService with
        member this.GetData() =
            let reader = new StringReader("<test>test</test>")
            let xr = XmlReader.Create(reader)
            
            let ver = OperationContext.Current.IncomingMessageVersion
            let msg = Message.CreateMessage(ver, "urn:IMyService/GetDataResponse", xr)
            printfn "%A\n" msg
            msg


let host = new ServiceHost(typeof<MyService>, new Uri("http://localhost:8000"))
host.Open()

let proxy = ChannelFactory<IMyService>.CreateChannel(host.Description.Endpoints.[0].Binding, host.Description.Endpoints.[0].Address)

let msg = proxy.GetData()
let xdr = msg.GetReaderAtBodyContents()
printfn "%O" <| xdr.ReadOuterXml()
xdr.Close()

(proxy :?> ICommunicationObject).Close()
host.Close()
