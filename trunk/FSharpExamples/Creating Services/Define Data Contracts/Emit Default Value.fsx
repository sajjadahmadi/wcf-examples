#r "System.Xml.Linq"
#r "System.Runtime.Serialization"
#r @"..\..\bin\Mcts70_503.dll"
open System
open System.Runtime.Serialization
Console.Clear()


[<DataContract(Name="Person")>]
type Person =
    { [<DataMember(EmitDefaultValue = true)>] // true is default
      mutable Name : string;
      
      [<DataMember(EmitDefaultValue = false)>]
      mutable Age : int }


printfn "Emit Default Name but not Age\n----------------------"
let p1 = { Name = null;
           Age   = 0 }
printfn "%s\n\n" (serialize p1)

printfn "When Not Default Value\n----------------------"
let p2 = { Name = "Ray Vernagus";
           Age   = 1 }
printfn "%s" (serialize p2)
