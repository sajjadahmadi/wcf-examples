#region

using System;
using System.Security;
using System.Security.Principal;
using System.ServiceModel;
using System.Threading;
using System.IdentityModel.Policy;
using System.Collections;
using System.IdentityModel.Claims;
using System.Collections.Generic;

#endregion

namespace ClaimsBasedServices
{
    [ServiceContract(Name = "SecureServiceContract", Namespace = "http://www.thatindigogirl.com/samples/2006/06")]
    internal interface ISecureService
    {
        [OperationContract]
        string SendMessage(string message);
    }

    public class SecureService : ISecureService
    {
        #region ISecureService Members

        string ISecureService.SendMessage(string message)
        {
            var birthDate = ClaimedBirthDate();
            ValidateBirthDate(birthDate);

            var identity = WindowsIdentity.GetCurrent();
            if (identity == null)
                throw new SecurityException("Current identity is null.");
            var username = identity.Name;
            var s =
                String.Format(
                    "Message '{0}' received. \r\n\r\nHost identity is {1}\r\n Security context PrimaryIdentity is {2}\r\n Security context WindowsIdentity is {3}\r\n Thread identity is {4}",
                    message, username, ServiceSecurityContext.Current.PrimaryIdentity.Name,
                    ServiceSecurityContext.Current.WindowsIdentity.Name, Thread.CurrentPrincipal.Identity.Name);

            return s;
        }

        private static void ValidateBirthDate(DateTime? birthDate)
        {
            if (birthDate == null)
                throw new SecurityException("Missing date of birth claim.");
            if (birthDate.Value.AddYears(13) > DateTime.Now)
                throw new SecurityException("User is too young to access this operation.");
        }

        private static DateTime? ClaimedBirthDate()
        {
            DateTime? birthDate = null;
            var authorizationContext = ServiceSecurityContext.Current.AuthorizationContext;
            foreach (var claimSet in authorizationContext.ClaimSets)
            {
                var claims = claimSet.FindClaims(ClaimTypes.DateOfBirth, Rights.PossessProperty);
                foreach (var claim in claims)
                    birthDate = Convert.ToDateTime(claim.Resource);
            }
            return birthDate;
        }

        #endregion
    }
}