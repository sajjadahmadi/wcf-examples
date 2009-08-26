#r "System.Xml.Linq"
#r "System.Runtime.Serialization"
#r @"..\..\bin\Mcts70_503.dll"
open System
open System.Runtime.Serialization
Console.Clear()


[<DataContract(Name="Person")>]
type Person =
    { [<DataMember(IsRequired=true)>]
      mutable Age : int;
      
      [<DataMember>]
      mutable Name : string }


let graph1 = "
<Person xmlns=\"http://schemas.datacontract.org/2004/07/\" xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\">
  <Age>35</Age>
  <Name>Ray Vernagus</Name>
</Person>"

let person = deserialize<Person> graph1
printfn "%A" person

// Cannot deserialize graph 2 because it's missing an Age element
let graph2 = "
<Person xmlns=\"http://schemas.datacontract.org/2004/07/\" xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\">
  <Name>Ray Vernagus</Name>
</Person>"

try
    deserialize<Person> graph2 |> ignore
with ex -> printfn "\n%s" ex.Message
