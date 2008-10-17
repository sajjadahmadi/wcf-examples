#light
#r "System.Xml.Linq"
#r "System.Runtime.Serialization"
#load "../../ref/Helpers.fsx"
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

let person = { new Person_Old with Name = "Ray" and Age = 35 }
let persondata = Helpers.serialize person
printfn "%s\n\n----------------------\n" persondata

let emp = Helpers.deserialize<Person_New> persondata
printfn "%A" emp