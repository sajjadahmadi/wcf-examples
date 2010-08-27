#r "System.ServiceModel"
#r @"..\bin\Mcts70_503.dll"
open System
open System.ServiceModel
open System.ServiceModel.Channels
open System.ServiceModel.Description
Console.Clear()


[<ServiceContract>]
type IMyContract =
    [<OperationContract>]
    abstract MyOperation : unit -> string


type MyService() =
    interface IMyContract with
        member this.MyOperation() = "My Message"


example2<MyService, IMyContract>
    (fun () ->
        let host = new ExampleHost<MyService, IMyContract>()
        let binding = host.Description.Endpoints.[0].Binding
        host.Description.Behaviors.Add(new ServiceMetadataBehavior())
        host.AddServiceEndpoint(typeof<IMetadataExchange>, binding, "mex") |> ignore
        host)

    (fun host _ ->
        let binding = host.Description.Endpoints.[1].Binding
        let address = host.Description.Endpoints.[1].Address.ToString()
        
        let mexClient = new MetadataExchangeClient(binding)
        let metadata = mexClient.GetMetadata(new Uri(address), MetadataExchangeClientMode.MetadataExchange)
        let importer = new WsdlImporter(metadata)

        let endpoints = importer.ImportAllEndpoints()
        printfn "%d Endpoint(s) Found\n--------------" endpoints.Count
        for endpoint in endpoints do
            printfn "  %s (%s): %A" endpoint.Name (endpoint.Binding.GetType().Name) endpoint.Address)
