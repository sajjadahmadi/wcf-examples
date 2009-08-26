#r "System.Xml.Linq"
#r "System.Runtime.Serialization"
#r @"..\bin\Mcts70_503.dll"
open System
open System.Runtime.Serialization
Console.Clear()


[<DataContract(Name="ContactType")>]
type ContactType =
    | [<EnumMember>] Customer = 0
    | [<EnumMember(Value="VendorName")>] Vendor   = 1
    | Partner  = 2
      

printfn "Normal Case (ContactType.Customer)\n-------------------"
printfn "%s\n" (serialize ContactType.Customer)
      
printfn "Overridden Value (ContactType.Vendor)\n-------------------"
printfn "%s\n" (serialize ContactType.Vendor)

printfn "Not Marked EnumMember (ContactType.Partner)\n-------------------"
try
    serialize ContactType.Partner |> ignore
with ex ->
    printfn "%s" ex.Message
