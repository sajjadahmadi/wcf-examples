using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Security.Permissions;
using System.ServiceModel.Channels;
using System.Security.Principal;
using System.Threading;
using System.Diagnostics;

namespace CodeRunner
{
    public class AuthorizationExample
    {
        // Contracts
        [ServiceContract]
        interface IBankAccounts
        {
            [OperationContract]
            void TransferMoney(double amount);
        }
        static class AppRoles
        {
            public const string Customers = @"MyDomain\Customers";
            public const string Tellers = @"MyDomain\Tellers";
        }

        // Service
        class BankService : IBankAccounts
        {
            // First line of defense: Must be a Customer OR Teller
            [PrincipalPermission(SecurityAction.Demand, Role = AppRoles.Customers)]
            [PrincipalPermission(SecurityAction.Demand, Role = AppRoles.Tellers)]
            public void TransferMoney(double amount)
            {
                IPrincipal principal = Thread.CurrentPrincipal;
                Debug.Assert(principal.Identity.IsAuthenticated);

                bool isCustomer = principal.IsInRole(AppRoles.Customers);
                bool isTeller = principal.IsInRole(AppRoles.Tellers);

                // Run-time condition
                if (isCustomer && !isTeller)
                {
                    if (amount > 5000) 
                    {
                        string message = "Customer not authorized to transfer this amount.";
                        throw new UnauthorizedAccessException(message);
                    }
                }

                // DoTransfer
            }
        }
    }
}
