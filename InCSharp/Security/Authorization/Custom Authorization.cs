using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.ServiceModel;
using System.IdentityModel.Policy;
using System.IdentityModel.Claims;
using System.Diagnostics;
using System.Security.Principal;
using System.Threading;

namespace CodeRunner
{
    [ServiceContract]
    interface IMyContract
    {
        [OperationContract]
        void MyOperation();
    }

    class MyService : IMyContract
    {
        public void MyOperation()
        {
            // Do something
        }
    }

    class MyAuthorizationPolicy : IAuthorizationPolicy
    {
        Guid id = Guid.NewGuid();

        string[] GetAllowedOperations(object user)
        {
            if (user.Equals("Domain\\Username"))
                return new string[] {
                    "http://example.org/MyService/MyOperation", 
                    "http://example.org/MyService/SomeOtherOperation"};
            else
                return new string[] { };
        }

        public string Id
        {
            get { return id.ToString(); }
        }


        public bool Evaluate(EvaluationContext evaluationContext, ref object state)
        {
            if (state != null)
                return true; // If state is null, then this method has already been called

            Debug.WriteLine("Inside MyAuthorizationPolicy.Evaluate");

            var claims = new List<Claim>();
            // Iterate through each of the claim sets in the evaluation context.
            foreach (var claimSet in evaluationContext.ClaimSets)
            {
                // Look for Name claims in the current claim set.
                foreach (var claim in claimSet.FindClaims(ClaimTypes.Name, Rights.PossessProperty))
                {
                    // Get the list of operations the given user (resource) is allowed to call.
                    var user = claim.Resource;
                    var ops = GetAllowedOperations(user);
                    foreach (var operation in ops)
                    {
                        // Add claims to the list
                        claims.Add(new Claim("http://example.org/claims/allowedoperation", operation, Rights.PossessProperty));
                        Debug.WriteLine("Claim added: " + operation);
                    }
                }
            }

            // Add claims to the evaluation context.
            evaluationContext.AddClaimSet(this, new DefaultClaimSet(this.Issuer, claims));

            // Will signifies that claims have been added.
            state = new object();

            return true;
        }

        public ClaimSet Issuer
        {
            get { return ClaimSet.System; }
        }

    }
    [TestClass]
    public class CustomAuthorizationTests
    {
        [TestMethod]
        public void UserIsAuthorized()
        {
            AppDomain.CurrentDomain.SetPrincipalPolicy(PrincipalPolicy.UnauthenticatedPrincipal);
            var roles = new string[] { "http://example.org/MyService/MyOperation" };
            var p = new GenericPrincipal(new GenericIdentity("Domain\\Username"), roles);
            Thread.CurrentPrincipal = p;
            CallOperation();
            // Operation is still getting called as the current user
        }

        [TestMethod]
        public void UserIsNotAuthorized()
        {
            CallOperation();
        }

        private static void CallOperation()
        {
            var uri = new Uri("net.tcp://localhost:8000");
            var binding = new NetTcpBinding();
            using (var host = new ServiceHost(typeof(MyService), uri))
            {
                host.AddServiceEndpoint(typeof(IMyContract), binding, "");

                // Add custom authorization policies
                var policies = new List<IAuthorizationPolicy>();
                policies.Add(new MyAuthorizationPolicy());
                host.Authorization.ExternalAuthorizationPolicies = policies.AsReadOnly();

                host.Open();

                var proxy = ChannelFactory<IMyContract>.CreateChannel(binding, new EndpointAddress(uri.ToString()));
                proxy.MyOperation();
                ((ICommunicationObject)proxy).Close();
            }
        }

    }
}
