#light
#r "System.Runtime.Serialization"
open System.Runtime.Serialization

type DataContractSerializer<'T>() =
    inherit XmlObjectSerializer()
    
    let serializer = new DataContractSerializer(typeof<'T>)

    override this.WriteObjectContent(writer: System.Xml.XmlDictionaryWriter, t: obj) =
        serializer.WriteObjectContent(writer, t)
    
    override this.WriteEndObject(writer: System.Xml.XmlDictionaryWriter) =
        serializer.WriteEndObject(writer)

    override this.WriteStartObject(writer: System.Xml.XmlDictionaryWriter, t: obj) =
        serializer.WriteStartObject(writer, t)
        
    override this.ReadObject(reader: System.Xml.XmlDictionaryReader, verifyObjectName: bool) =
        serializer.ReadObject(reader, verifyObjectName)
        
    override this.IsStartObject(reader: System.Xml.XmlDictionaryReader) =
        serializer.IsStartObject(reader)
    
    [<OverloadID("Read.Stream")>]
    member this.Read(stream: System.IO.Stream) =
        serializer.ReadObject(stream) :?> 'T

    [<OverloadID("Read.XmlReader")>]
    member this.Read(reader: System.Xml.XmlReader) =
        serializer.ReadObject(reader)

    [<OverloadID("Write.Stream")>]
    member this.Write(stream: System.IO.Stream, t: 'T) =
        serializer.WriteObject(stream, t)

    [<OverloadID("Write.XmlWriter")>]
    member this.Write(writer: System.Xml.XmlWriter, t: 'T) =
        serializer.WriteObject(writer, t)

[<DataContract>]
type Person =
    { [<DataMember>] mutable FirstName: string;
      [<DataMember>] mutable LastName: string }

let serializer = new DataContractSerializer<Person>()
let stream = new System.IO.MemoryStream()
let p = { FirstName = "Ray"; LastName = "Vernagus" }

serializer.Write(stream, p)
stream.Position <- int64 0

let reader = new System.IO.StreamReader(stream)
printfn "%s" (reader.ReadToEnd())
stream.Position <- int64 0

let p2 = serializer.Read(stream)
printfn "%A" p2
