using System.Drawing;
using System.Net;
using System.Threading.Tasks;

namespace Kontur.ImageTransformer.Interfases
{
    public interface IRequest
    {
        HttpListenerContext Context { get; }
        string Filter { get; }
        int[] Coords { get; }
        Bitmap Image { get; }
        bool IsValidated { get; }

        Task<IResponse> Process(IRequest request);
    }
}