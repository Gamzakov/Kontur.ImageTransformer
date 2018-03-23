using System;
using System.Drawing;
using System.Net;
using System.Threading.Tasks;
using Kontur.ImageTransformer.Interfases;

namespace Kontur.ImageTransformer
{
    public abstract class BaseRequest : IRequest, IDisposable
    {
        public abstract HttpListenerContext Context { get; }
        public abstract bool IsValidated { get; }
        public abstract string Filter { get; }
        public abstract int[] Coords { get; }
        public abstract Bitmap Image { get; }

        public abstract Task<IResponse> Process(IRequest request);

        protected abstract Bitmap GetImage(HttpListenerRequest request);
        protected abstract Uri GetUrl(HttpListenerContext context);
        protected abstract string GetFilter(Uri url);
        protected abstract bool UrlIsValid(Uri url);
        protected abstract int[] GetCoords(Uri url);
        protected abstract bool IsValid();

        public abstract void Dispose();
    }
}