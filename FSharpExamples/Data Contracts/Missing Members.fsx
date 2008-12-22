#light
#r "System.Xml.Linq"
#r "System.Runtime.Serialization"
#load "Serialization.fsx"
open System
open System.Runtime.Serialization

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

let pOld = { new Person_Old with Name = "Ray" and Age = 35 }
let pOldData = Serialization.serialize pOld
printfn "%s\n\n----------------------\n" pOldData

let pNew = Serialization.deserialize<Person_New> pOldData
printfn "%A" pNew