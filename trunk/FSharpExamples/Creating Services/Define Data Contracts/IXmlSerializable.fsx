#r "System.Xml"
#r "System.ServiceModel"
#r "System.Runtime.Serialization"
#r @"..\..\bin\Mcts70_503.dll"
open System
open System.IO
open System.Xml
open System.Xml.Schema
open System.Xml.Serialization
open System.Runtime.Serialization
open System.ServiceModel
open System.ServiceModel.Description
Console.Clear()


[<DataContract(Name = "Item", Namespace = "http://schemas.myexample.org")>]
type Item =
    { [<DataMember>] mutable Name : string }


[<XmlSchemaProvider("GetSchema")>]
type ItemSerializer(item : Item) =
    let mutable item' = item
    let ns = "http://schemas.myexample.org"
    
    new() = ItemSerializer({ Name = "" })
    
    member this.Item
        with get() = item'
        and set v = item' <- v

    static member GetSchema(schemaSet : XmlSchemaSet) =
        let ns = "http://schemas.myexample.org"
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
                if reader.IsStartElement("Name", ns) then
                    reader.MoveToContent() |> ignore
                    item.Name <- reader.ReadString()
                    reader.MoveToContent() |> ignore
                    reader.ReadEndElement()
                    
            this.Item <- item
        
        member this.WriteXml(writer : XmlWriter) =
            writer.WriteElementString("Name", ns, this.Item.Name)


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


let item = { Name = "Client Item" }

example2<MyService, IMyContract>
    (fun host -> host.EnableHttpGet())
    (fun host _ ->
        let proxy = host.CreateProxyOf<IMyContractClient>()
        // Client uses default DataContractSerializer
        // Server manually deserializes
        proxy.MyMethod(item)
        let serverItem = proxy.MyOtherMethod()
        printfn "Client Received: %A" serverItem

        printfn "\n\nVisit http://localhost?wsdl for metadata"
        printfn "Visit http://localhost?xsd=xsd2 for Item schema"
        printfn "Press <ENTER> to end the example..."
        Console.ReadKey() |> ignore)
