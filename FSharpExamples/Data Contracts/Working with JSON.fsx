#light
#r "System.Xml.Linq"
#r "System.Runtime.Serialization"
#r "System.ServiceModel.Web"
open System
open System.IO
open System.Xml.Linq
open System.Runtime.Serialization
open System.Runtime.Serialization.Json


let deserialize<'a> (x: string) =
    let stream = new MemoryStream()
    let data = System.Text.Encoding.UTF8.GetBytes(x)
    stream.Write(data, 0, data.Length)
    stream.Position <- 0L
    let deserializer = new DataContractJsonSerializer(typeof<'a>)
    deserializer.ReadObject(stream) :?> 'a
    
    
[<DataContract(Name="Person", Namespace="")>]
type Person =
    { [<DataMember(Name = "name")>]
      mutable Name : string;
      
      [<DataMember(Name = "age")>]
      mutable Age : int }

let personData = "{\"name\":\"Ray\",\"age\":36}"
let emp = deserialize<Person> personData
printfn "%A" emp