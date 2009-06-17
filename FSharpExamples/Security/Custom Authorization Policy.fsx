#r "System.ServiceModel"
#r "System.IdentityModel"
#r "System.Runtime.Serialization"
open System
open System.Collections.Generic
open System.IdentityModel.Claims
open System.IdentityModel.Policy
open System.ServiceModel
open System.ServiceModel.Description
open System.ServiceModel.Security
Console.Clear()


[<ServiceContract>]
type IMyContract =
    [<OperationContract>]
    abstract MyOperation : unit -> string


type MyService() =
    interface IMyContract with
        member this.MyOperation() =
            "result"


type MyCustomValidator() =
    let id = Guid.NewGuid()
    
    member private this.GetAllowedOperations(user) =
        printfn "Adding allowed operation(s) for %s" user
        // Return allowed operations here
        [ "http://example.org/MyService/MyOperation";
          "http://example.org/MyService/SomeOtherOperation" ]

    interface IAuthorizationPolicy with
        member this.Id = string id
    
        member this.Evaluate(evaluationContext, state) =
            // Check to see if this method has been called already
            printfn "%A" state
            if state = null then
                state <- new obj()
                let claims = new List<Claim>()
                for claimSet in evaluationContext.ClaimSets do
                    // Look for Name claims in the current claim set
                    for claim in claimSet.FindClaims(ClaimTypes.Name, Rights.PossessProperty) do
                        // Get the list of operations the given user is allowed to call
                        let ops = this.GetAllowedOperations(string claim.Resource)
                        for op in ops do
                            claims.Add(new Claim("http://example.org/claims/allowedoperation", op, Rights.PossessProperty))
                            printfn "  Claim added: %s" op
                evaluationContext.AddClaimSet(this, new DefaultClaimSet((this :> IAuthorizationPolicy).Issuer, claims))
                ()
            true
            
        member this.Issuer = ClaimSet.System


let uri = new Uri("net.tcp://localhost:8000")
let binding = new NetTcpBinding()
let host = new ServiceHost(typeof<MyService>, uri)
host.AddServiceEndpoint(typeof<IMyContract>, binding, "")

// Add custom authorization policies here
let policies = new List<IAuthorizationPolicy>()
policies.Add(new MyCustomValidator())
host.Authorization.ExternalAuthorizationPolicies <- policies.AsReadOnly()

host.Open()

let proxy = ChannelFactory<IMyContract>.CreateChannel(binding, new EndpointAddress(string uri))
proxy.MyOperation()
proxy.MyOperation()

(proxy :?> ICommunicationObject).Close()
host.Close()