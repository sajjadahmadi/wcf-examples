#light
#r "System.Xml"
#r "System.ServiceModel"
#r "System.Runtime.Serialization"
open System
open System.IO
open System.Xml
open System.Xml.Schema
open System.Xml.Serialization
open System.Runtime.Serialization
open System.ServiceModel


[<DataContract(Name = "Item", Namespace = "")>]
type Item =
    { [<DataMember>] mutable Name : string }


[<XmlSchemaProvider("GetSchema")>]
type ItemSerializer(item : Item) =
    let mutable item' = item
    
    new() = ItemSerializer({ Name = "" })
    
    member this.Item
        with get() = item'
        and set v = item' <- v

    static member GetSchema(schemaSet : XmlSchemaSet) =
        let ns = "http://www.thatindigogirl.com/samples/2006/06"
        let schemaString = sprintf "<xs:schema xmlns:tns='%s' xmlns:xs='http://www.w3.org/2001/XMLSchema' targetNamespace='%s' elementFormDefault='qualified' attributeFormDefault='unqualified'><xs:complexType name='Item'><xs:sequence><xs:element name='Name' type='xs:string' nillable='false'/></xs:sequence></xs:complexType></xs:schema>" ns ns

        let schema = XmlSchema.Read(new StringReader(schemaString), null)
        schemaSet.XmlResolver <- new XmlUrlResolver()
        schemaSet.Add(schema) |> ignore
        new XmlQualifiedName("Item", ns)

    interface IXmlSerializable with
        member this.GetSchema() =
            raise (new NotImplementedException())
        
        member this.ReadXml(reader : XmlReader) =
            let item = { Name = "" }
            while reader.IsStartElement() do
                reader.MoveToContent() |> ignore
                reader.Read() |> ignore
                if reader.IsStartElement("Name") then
                    reader.MoveToContent() |> ignore
                    item.Name <- reader.ReadString()
                    reader.MoveToContent() |> ignore
                    reader.ReadEndElement()
                    
            this.Item <- item
        
        member this.WriteXml(writer : XmlWriter) =
            let ns = "http://www.thatindigogirl.com/samples/2006/06"
            writer.WriteElementString("Name", ns, this.Item.Name)


[<ServiceContract(Name = "IMyContract")>]
type IMyContract =
    [<OperationContract>]
    abstract MyMethod : ItemSerializer -> unit


[<ServiceContract(Name = "IMyContract")>]
type IMyContractClient =
    [<OperationContract>]
    abstract MyMethod : Item -> unit


[<ServiceBehavior(IncludeExceptionDetailInFaults = true)>]
type MyService() =
    interface IMyContract with
        member this.MyMethod(itemSerializer) =
            let item = itemSerializer.Item
            printfn "Service Read: %A" item


let uri = new Uri("net.tcp://localhost")
let binding = new NetTcpBinding()
let host = new ServiceHost(typeof<MyService>, [| uri |])
host.AddServiceEndpoint(typeof<IMyContract>, binding, "")
host.Open()

let proxy = ChannelFactory<IMyContractClient>.CreateChannel(binding, new EndpointAddress(string uri))
let item = { Name = "Messenger Bag" }
proxy.MyMethod(item)

