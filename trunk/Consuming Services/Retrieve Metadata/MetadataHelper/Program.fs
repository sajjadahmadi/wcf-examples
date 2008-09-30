#light
open System
open System.Diagnostics
open MetadataHelper

printfn "Address of service:"
let url = Console.ReadLine()

printfn "Contract namespace:"
let ns = Console.ReadLine()

printfn "Contract name:"
let n = Console.ReadLine()

let supported = MetadataHelper.queryContract url ns n
if supported
    then printfn "OK!"
    else printfn "Is NOT supported."
do Console.ReadKey(true) |> ignore