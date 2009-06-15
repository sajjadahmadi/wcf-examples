using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.ServiceModel;
using System.IdentityModel.Policy;
using System.IdentityModel.Claims;

namespace CodeRunner
{
    [ServiceContract]
    interface IMyContract
    {
        [OperationContract]
        string MyOperation();
    }

    class MyService : IMyContract
    {
        public string MyOperation()
        {
            return "result";
        }
    }

    enum ClaimStates
    {
        ClaimsAdded
    }

    class MyCustomValidator : IAuthorizationPolicy
    {
        ClaimStates ClaimState = ClaimStates.ClaimsAdded;

        Guid id = Guid.NewGuid();

        string[] GetAllowedOperations(object user)
        {
            return new string[] {
                "http://example.org/MyService/MyOperation", 
                "http://example.org/MyService/SomeOtherOperation"};
        }

        string IAuthorizationComponent.Id
        {
            get { return id.ToString(); }
        }

        bool IAuthorizationPolicy.Evaluate(EvaluationContext evaluationContext, ref object state)
        {
            if (state != null)
                return true; // What is this for?
            state = ClaimStates.ClaimsAdded;
            var claims = new List<Claim>();
            foreach (var claimSet in evaluationContext.ClaimSets)
            {
                foreach (var claim in claimSet.FindClaims(ClaimTypes.Name, Rights.PossessProperty))
                {
                    var ops = GetAllowedOperations(claim.Resource);
                    foreach (var op in ops)
                    {
                        claims.Add(new Claim("http://example.org/claims/allowedoperation", op, Rights.PossessProperty));
                    }
                }
            }
            return true;
        }

        ClaimSet IAuthorizationPolicy.Issuer
        {
            get { return ClaimSet.System; }
        }

        [TestMethod]
        void TestThis()
        {
            
            var uri = new Uri("net.tcp://localhost:8000");
            var binding = new NetTcpBinding();
            var host = new ServiceHost(typeof(MyService), uri);
            host.AddServiceEndpoint(typeof(IMyContract), binding, "");

            // Add custom authorization policies
            var policies = new List<IAuthorizationPolicy>();
            policies.Add(new MyCustomValidator());
            host.Authorization.ExternalAuthorizationPolicies = policies.AsReadOnly();

            host.Open();

            Assert.Inconclusive("Incomplete");
        }
    }
}
