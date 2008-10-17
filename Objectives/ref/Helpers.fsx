#light
#r "System.Runtime.Serialization"
#r "System.Xml.Linq"
open System.IO
open System.Xml.Linq
open System.Runtime.Serialization

let serialize<'a> (x: 'a) =
    let serializer = new DataContractSerializer(typeof<'a>)
    let stream = new MemoryStream()
    serializer.WriteObject(stream, x)
    stream.Position <- 0L
    let reader = new StreamReader(stream)
    let doc = XDocument.Parse(reader.ReadToEnd())
    doc.ToString()

let deserialize<'a> (x: string) =
    let stream = new MemoryStream()
    let data = System.Text.Encoding.UTF8.GetBytes(x)
    stream.Write(data, 0, data.Length)
    stream.Position <- 0L
    let deserializer = new DataContractSerializer(typeof<'a>)
    deserializer.ReadObject(stream)
