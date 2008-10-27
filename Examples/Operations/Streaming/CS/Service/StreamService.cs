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
            //stream = new MemoryStream();
            //StreamWriter writer = new StreamWriter(stream);
            //writer.WriteLine("This is a test...");
            //stream.Position = 0;

            return stream;
        }

        #endregion
    }
}
