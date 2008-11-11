using System.Transactions;

namespace System.ServiceModel.Examples
{
    // Use the client's isolation level if one exists, otherwise Serializable
    // Serializable - The highest degree of isolation
    [ServiceBehavior(TransactionIsolationLevel = IsolationLevel.Unspecified)]
    partial class MyService
    {
    }
}