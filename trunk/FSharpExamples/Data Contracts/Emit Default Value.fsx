#r "System.Xml.Linq"
#r "System.Runtime.Serialization"
#load "Serialization.fs"
open System
open System.Runtime.Serialization
Console.Clear()


[<DataContract(Name="Person")>]
type Person =
    { [<DataMember>] // EmitDefaultValue=true
      mutable Name : string;
      
      [<DataMember(EmitDefaultValue=false)>]
      mutable Age : int }


let p1 = { Name = null;
           Age   = 0 }
printfn "%s\n\n----------------------\n" (Serialization.serialize p1)

let p2 = { Name = "Ray Vernagus";
           Age   = 1 }
printfn "%s" (Serialization.serialize p2)
