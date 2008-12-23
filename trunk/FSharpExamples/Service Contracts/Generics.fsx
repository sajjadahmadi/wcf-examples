#light
#r "System.Xml.Linq"
#r "System.Configuration"
#r "System.ServiceModel"
#r "System.Runtime.Serialization"
open System
open System.ServiceModel
open System.ServiceModel.Configuration
open System.ServiceModel.Description
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


let uri = new Uri("net.tcp://localhost")
let binding = new NetTcpBinding()
let host = new ServiceHost(typeof<MyServiceHost>, [| uri |])
host.Description.Behaviors.Add(new ServiceMetadataBehavior())
host.AddServiceEndpoint(typeof<IMyContract>, binding, "")
host.AddServiceEndpoint(typeof<IMetadataExchange>, binding, "mex")
host.Open()

host.Description.Endpoints
|> Seq.iter (fun ep -> printfn "  %A" ep.Address.Uri)

let mexClient = new MetadataExchangeClient(binding)
let metadata = mexClient.GetMetadata(new EndpointAddress("net.tcp://localhost/mex"))
let importer = new WsdlImporter(metadata)
let endpoints = importer.ImportAllEndpoints()

let ep = endpoints |> Seq.nth 0
printfn "%s" ep.Contract.Name
printfn "  %s" ep.Contract.Operations.[0].Name
printfn "  %A" ep.Contract.Operations.[0].Messages.[0].Body.Parts.[0].Name

host.Close()
