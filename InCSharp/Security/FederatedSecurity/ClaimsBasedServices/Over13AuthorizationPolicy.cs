using System;
using System.IdentityModel.Claims;
using System.IdentityModel.Policy;
using System.ServiceModel;

namespace ClaimsBasedServices
{
	class Over13AuthorizationPolicy : IAuthorizationPolicy
	{
		private readonly string _id;
		private readonly ClaimSet _issuer;

		public Over13AuthorizationPolicy()
		{
			_id = Guid.NewGuid().ToString();
			_issuer = ClaimSet.System; // Current app is the issuer; app-trusted issuer
		}

		bool IAuthorizationPolicy.Evaluate(EvaluationContext evaluationContext, ref object state)
		{
			DateTime? birthDate = null;
			var authorizationContext = ServiceSecurityContext.Current.AuthorizationContext;
			foreach (var claimSet in authorizationContext.ClaimSets)
			{
				var claims = claimSet.FindClaims(ClaimTypes.DateOfBirth, Rights.PossessProperty);
				foreach (var claim in claims)
					birthDate = Convert.ToDateTime(claim.Resource);
			}
			return birthDate.HasValue 
				? birthDate.Value <= DateTime.Now.AddYears(-13) 
				: false;
		}

		ClaimSet IAuthorizationPolicy.Issuer
		{
			get { return _issuer; }
		}

		string IAuthorizationComponent.Id
		{
			get { return _id; }
		}
	}
}
