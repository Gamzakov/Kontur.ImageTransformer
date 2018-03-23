using System.Drawing;
using System.Net;

namespace Kontur.ImageTransformer.Interfases
{
    public interface IResponse
    {
        HttpListenerResponse Context { get; }
        HttpStatusCode ResponseCode { get; }
        Bitmap Image { get; }

        byte[] GetBytesOfImage();
    }
}