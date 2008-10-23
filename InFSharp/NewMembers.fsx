#light
#r "System.Xml.Linq"
#r "System.Runtime.Serialization"
#load "Helpers.fsx"
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
    
    member this.ExtensionData with get() = extData
                              and set v = extData <- v
    
    interface IExtensibleDataObject with
        member this.ExtensionData with get() = extData
                                  and set v = extData <- v

let pNew = { Name = "Ray"; Age = 35; Address = "123 MAIN ST" }
let pNewData = Helpers.serialize pNew
printfn "%s\n\n----------------------\n" pNewData

let pOld = Helpers.deserialize<Person_Old> pNewData
printfn "{ Name = %s; Age = %d }\n" pOld.Name pOld.Age

// Round Trip
let pOldData = Helpers.serialize pOld
printfn "%s\n\n----------------------\n" pOldData
let pNewRoundTrip = Helpers.deserialize<Person_New> pOldData
printfn "%A" pNewRoundTrip

