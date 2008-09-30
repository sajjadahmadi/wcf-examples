#light
open System
open System.ServiceModel
open System.ServiceModel.Channels
open System.ServiceModel.Description
open System.Text.RegularExpressions

module MetadataHelper =

    let internal messageMultiplier = 5

    let internal (|NetTcp|NetPipe|Http|Https|) = function
        | "net.tcp"  -> NetTcp (new TcpTransportBindingElement())
        | "net.pipe" -> NetPipe (new NamedPipeTransportBindingElement())
        | "http"     -> Http (new HttpTransportBindingElement())
        | "https"    -> Https (new HttpsTransportBindingElement())
        | _          -> failwith "invalid scheme"

    let queryMexEndpoint (mexAddress: string) (bindingElement: BindingElement) =
        let binding = new CustomBinding([| bindingElement |])
        
        let mexClient = new MetadataExchangeClient(binding)
        try
            let metadata = mexClient.GetMetadata(new EndpointAddress(mexAddress))
            
            let importer = new WsdlImporter(metadata)
            Some (importer.ImportAllEndpoints())
        with _ -> None

    let getEndpoints (mexAddress: string) =
        let address = new Uri(mexAddress)
        
        let setMessageSize (el: TransportBindingElement) =
            el.MaxReceivedMessageSize <- el.MaxReceivedMessageSize * (int64 messageMultiplier)
        
        let httpGetAddr (addr: string) =
            match Regex.IsMatch("?wsdl$", addr, RegexOptions.IgnoreCase) with
            | true  -> addr
            | false -> addr + "?wsdl"
        
        let useHttpGet (el: BindingElement) =
            let addr = httpGetAddr mexAddress
            let binding = new CustomBinding([| el |])
            let mexClient = new MetadataExchangeClient(binding)
            let metadata = mexClient.GetMetadata(new Uri(addr), MetadataExchangeClientMode.HttpGet)
            let importer = new WsdlImporter(metadata)
            importer.ImportAllEndpoints()
        
        match address.Scheme with
        | NetTcp el  ->
            setMessageSize el
            queryMexEndpoint mexAddress el
        | NetPipe el ->
            setMessageSize el
            queryMexEndpoint mexAddress el
        | Http el    ->
            setMessageSize el
            match queryMexEndpoint mexAddress el with
            | None -> Some (useHttpGet el)
            | es   -> es
        | Https el   ->
            setMessageSize el
            match queryMexEndpoint mexAddress el with
            | None -> Some (useHttpGet el)
            | es   -> es
    
    let internal serviceName<'C> =
        let t = typeof<'C>
        let attributes = t.GetCustomAttributes(typeof<ServiceContractAttribute>, false)
        
        if not t.IsInterface then None
        else if attributes.Length = 0 then None
        else
            let attr = attributes.[0] :?> ServiceContractAttribute
            match attr.Name,attr.Namespace with
            | null,null -> Some (t.ToString(), "http://tempuri.org")
            | null,ns   -> Some (t.ToString(), ns)
            | n,null    -> Some (n, "http://tempuri.org")
            | n,ns      -> Some (n, ns)
                
    let queryContract (mexAddress: string) (contractNamespace: string) (contractName: string) =
       
        let matches (endpoint: ServiceEndpoint) =
            endpoint.Contract.Namespace = contractNamespace &&
            endpoint.Contract.Name = contractName
    
        if String.IsNullOrEmpty(contractNamespace) then false
        else if String.IsNullOrEmpty(contractName) then false
        else
            match getEndpoints mexAddress with
            | None    -> false
            | Some es ->
                es
                |> Seq.exists matches

    let queryContractType<'C> (mexAddress: string) =
        match serviceName<'C> with
        | None        -> false
        | Some (n,ns) ->
            queryContract mexAddress n ns
