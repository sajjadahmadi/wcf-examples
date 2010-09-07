#r "System.ServiceModel"
#r @"..\bin\Mcts70_503.dll"
open System
open System.ServiceModel
open System.ServiceModel.Channels
open System.ServiceModel.Description


[<ServiceContract>]
type IMyContract =
    [<OperationContract>]
    abstract MyOperation : unit -> string


type MyService() =
    interface IMyContract with
        member this.MyOperation() = "My Message"


let host = new ServiceHost(typeof<MyService>, new Uri("http://localhost"))
host.Description.Behaviors.Add(new ServiceMetadataBehavior(HttpGetEnabled = true))
host.Open()

let mexClient = new MetadataExchangeClient(host.Description.Endpoints.[0].Binding)
let metadata = mexClient.GetMetadata(new Uri(host.Description.Endpoints.[0].Address.ToString() + "?wsdl"), MetadataExchangeClientMode.HttpGet)
let importer = new WsdlImporter(metadata)

let endpoints = importer.ImportAllEndpoints()
printfn "%d Endpoint(s) Found\n--------------" endpoints.Count
for endpoint in endpoints do
    printfn "  %s (%s): %A" endpoint.Name (endpoint.Binding.GetType().Name) endpoint.Address
