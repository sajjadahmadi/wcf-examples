using System.ServiceModel;
using System.IO;

namespace Service
{
    [ServiceContract]
    public interface IStreamServiceContract
    {
        [OperationContract]
        Stream EchoStream(Stream stream);
    }
    
    public class StreamService : IStreamServiceContract
    {
        #region IStreamServiceContract Members

        public Stream EchoStream(Stream stream)
        {
            return stream;
        }

        #endregion
    }
}
