#light
#r "System.Runtime.Serialization"
open System.Runtime.Serialization

[<DataContract>]
type Person =
    { [<DataMember>] mutable FirstName: string;
      [<DataMember>] mutable LastName: string }

let serializer = new DataContractSerializer(typeof<Person>)
let netSerializer = new NetDataContractSerializer()

let stream = new System.IO.MemoryStream()
let p = { FirstName = "Ray"; LastName = "Vernagus" }

netSerializer.WriteObject(stream, p)
stream.Position <- int64 0

let reader = new System.IO.StreamReader(stream)
printfn "%s" (reader.ReadToEnd())
stream.Position <- int64 0

let p2 = serializer.ReadObject(stream)
printfn "%A" p2

// Can't go the other way
let stream2 = new System.IO.MemoryStream()
serializer.WriteObject(stream2, p)
stream2.Position <- int64 0

let reader2 = new System.IO.StreamReader(stream2)
printfn "%s" (reader2.ReadToEnd())
stream2.Position <- int64 0

let p3 = netSerializer.ReadObject(stream2)
printfn "%A" p3
