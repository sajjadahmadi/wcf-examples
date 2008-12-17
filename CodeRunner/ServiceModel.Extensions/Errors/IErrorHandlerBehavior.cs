using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace System.ServiceModel.Errors
{
    public interface IErrorHandlerBehavior : IErrorHandler, IServiceBehavior
    {
        Type ServiceType { get; }
    }
}