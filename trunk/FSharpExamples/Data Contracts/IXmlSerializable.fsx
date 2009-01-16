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
        let schemaString = sprintf "<xs:schema xmlns:tns='http://tempuri.org' xmlns:xs='http://www.w3.org/2001/XMLSchema' targetNamespace='http://tempuri.org' elementFormDefault='qualified' attributeFormDefault='unqualified'><xs:complexType name='Item'><xs:sequence><xs:element name='Name' type='xs:string' nillable='false'/></xs:sequence></xs:complexType></xs:schema>"

        let schema = XmlSchema.Read(new StringReader(schemaString), null)
        schemaSet.XmlResolver <- new XmlUrlResolver()
        schemaSet.Add(schema) |> ignore
        new XmlQualifiedName("Item", "")

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
            writer.WriteElementString("Name", "", this.Item.Name)


[<ServiceContract(Name = "IMyContract")>]
type IMyContract =
    [<OperationContract>]
    abstract MyMethod : ItemSerializer -> unit // Note different signature in server vs. client contract
    
    [<OperationContract>]
    abstract MyOtherMethod : unit -> ItemSerializer


[<ServiceContract(Name = "IMyContract")>]
type IMyContractClient =
    [<OperationContract>]
    abstract MyMethod : Item -> unit
    
    [<OperationContract>]
    abstract MyOtherMethod : unit -> Item


[<ServiceBehavior(IncludeExceptionDetailInFaults = true)>]
type MyService() =
    interface IMyContract with
        member this.MyMethod(itemSerializer) =
            let item = itemSerializer.Item
            printfn "Service Read: %A" item
        
        member this.MyOtherMethod() =
            let item = { Name = "Server Item" }
            new ItemSerializer(item)


let uri = new Uri("net.tcp://localhost")
let binding = new NetTcpBinding()
let host = new ServiceHost(typeof<MyService>, [| uri |])
host.AddServiceEndpoint(typeof<IMyContract>, binding, "")
host.Open()

let proxy = ChannelFactory<IMyContractClient>.CreateChannel(binding, new EndpointAddress(string uri))
let item = { Name = "Client Item" }
// Client uses default DataContractSerializer
// Server manually deserializes
proxy.MyMethod(item)

let serverItem = proxy.MyOtherMethod()
printfn "Client Received: %A" serverItem
