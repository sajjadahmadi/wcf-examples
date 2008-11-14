#light
#r "System.Xml.Linq"
#r "System.ServiceModel"
#r "System.Runtime.Serialization"
#load "MetadataHelper.fsx"
#load "InProcHost.fsx"
open Mcts_70_503
open System
open System.ServiceModel
open System.ServiceModel.Configuration
open System.Runtime.Serialization

[<DataContract>]
type MyClass<'a> =
    { [<DataMember>] mutable MyMember : 'a }
    
[<ServiceContract>]
type IMyContract =
    [<OperationContract>]
    abstract MyMethod : MyClass<int> -> unit

type MyServiceHost() =
    interface IMyContract with
        member this.MyMethod(x) = ()

let host = new InProcHost<MyServiceHost>()
host.AddEndpoint<IMyContract>(new NetNamedPipeBinding())
host.EnableMetadataExchange()
host.Open()

let endpoints = MetadataHelper.MetadataHelper.getEndpoints "net.pipe://localhost/mex"
match endpoints with
| None     -> failwith "No endpoints..."
| Some eps ->
    let ep = eps |> Seq.nth 0
    printfn "%s" ep.Contract.Name
    printfn "  %s" ep.Contract.Operations.[0].Name
    printfn "  %A" ep.Contract.Operations.[0].Messages.[0].Body.Parts.[0].Name

host.Close()