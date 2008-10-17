#light
#r "System.Xml.Linq"
#r "System.Runtime.Serialization"
#load "../../ref/Helpers.fsx"
open System
open System.Runtime.Serialization

[<DataContract(Name="Person")>]
type Person_New =
    { [<DataMember>]
      mutable Name : string;
      
      [<DataMember>]
      mutable Age : int;
      
      [<DataMember>]
      mutable Address : string }
      
[<DataContract(Name="Person")>]
type Person_Old() =
    let mutable name: string = null
    let mutable age: int = 0
    let mutable extData: ExtensionDataObject = null
    
    [<DataMember>]    
    member this.Name with get () = name
                     and set v = name <- v
      
    [<DataMember>]
    member this.Age with get () = age
                    and set v = age <- v
    
    interface IExtensibleDataObject with
        member this.ExtensionData with get () = extData
                                  and set v = extData <- v

let person = { Name = "Ray"; Age = 35; Address = "123 MAIN ST" }
let persondata = Helpers.serialize person
printfn "%s\n\n----------------------\n" persondata

let pserver = Helpers.deserialize<Person_Old> persondata
let pdata = pserver :> IExtensibleDataObject
printfn "%A" pdata.ExtensionData