#light
#r "System.Xml.Linq"
#r "System.Runtime.Serialization"
#load "../../ref/Helpers.fsx"
open System
open System.Runtime.Serialization

[<DataContract(Name="Person")>]
type Person =
    { [<DataMember>]
      mutable Name : string;
      
      [<DataMember(IsRequired=true)>]
      mutable Age : int }

let graph1 = "<Person xmlns=\"http://schemas.datacontract.org/2004/07/\" xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\">
  <Age>35</Age>
  <Name>Ray Vernagus</Name>
</Person>"

let p1 = Helpers.deserialize<Person> graph1
printfn "%A" p1

// Cannot deserialize graph 2 because it's missing an Age element
let graph2 = "<Person xmlns=\"http://schemas.datacontract.org/2004/07/\" xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\">
  <Name>Ray Vernagus</Name>
</Person>"

try
    Helpers.deserialize<Person> graph2 |> ignore
with ex -> printfn "%s" ex.Message
