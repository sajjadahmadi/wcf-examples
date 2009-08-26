#r "System.Xml.Linq"
#r "System.Runtime.Serialization"
#r @"..\..\bin\Mcts70_503.dll"
open System
open System.Runtime.Serialization
Console.Clear()


[<DataContract(Name="Person")>]
type Person_Old =
    { [<DataMember>]
      mutable Name : string;
      
      [<DataMember>]
      mutable Age : int }


[<DataContract(Name="Person")>]
type Person_New =
    { [<DataMember>]
      mutable Name : string;
      
      [<DataMember>]
      mutable Age : int
      
      [<DataMember>]
      mutable Address : string } with
    
      [<OnDeserializing>]
      member this.OnDeserializing(context: StreamingContext) =
        this.Address <- "N/A"


let pOld = { Name = "Ray"; Age = 35 } : Person_Old
let pOldData = serialize pOld
printfn "%s\n\n----------------------\n" pOldData

let pNew = deserialize<Person_New> pOldData
printfn "%A" pNew