using System.ServiceModel.Dispatcher;
using System.ServiceModel.Description;

namespace System.ServiceModel.Errors
{
    public interface IErrorHandlerBehavior : IErrorHandler, IServiceBehavior
    {
        Type ServiceType { get; }
    }
}