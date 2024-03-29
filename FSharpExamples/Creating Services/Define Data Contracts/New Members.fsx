#r "System.Runtime.Serialization"
#r @"..\..\bin\Mcts70_503.dll"
open System
open System.Runtime.Serialization
Console.Clear()


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
let pNewData = serialize pNew
printfn "%s\n\n----------------------\n" pNewData

let pOld = deserialize<Person_Old> pNewData
printfn "{ Name = %s; Age = %d }\n" pOld.Name pOld.Age

// Round Trip
let pOldData = serialize pOld
printfn "%s\n\n----------------------\n" pOldData
let pNewRoundTrip = deserialize<Person_New> pOldData
printfn "%A" pNewRoundTrip

