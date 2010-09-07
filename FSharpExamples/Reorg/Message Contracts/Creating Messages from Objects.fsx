#r "System.ServiceModel"
#r "System.Runtime.Serialization"
open System
open System.Runtime.Serialization
open System.ServiceModel
open System.ServiceModel.Channels


[<DataContract(Name = "Person", Namespace = "")>]
type Person =
    { [<DataMember>] 
      mutable Name : string

      [<DataMember>] 
      mutable Age : int }


[<ServiceContract(Namespace = "")>]
type IMyService =
    [<OperationContract>]
    abstract GetData : unit -> Message


type MyService() =
    interface IMyService with
        member this.GetData() =
            let p = { Name = "Ray"; Age = 37 }
            
            let ver = OperationContext.Current.IncomingMessageVersion
            let msg = Message.CreateMessage(ver, "urn:IMyService/GetDataResponse", p)
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
