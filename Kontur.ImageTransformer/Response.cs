using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using Kontur.ImageTransformer.Interfases;

namespace Kontur.ImageTransformer
{
    public sealed class Response : IResponse, IDisposable
    {
        public HttpListenerResponse Context { get; }
        public HttpStatusCode ResponseCode { get; }
        public Bitmap Image { get; }

        public Response(HttpListenerResponse context, HttpStatusCode code, Bitmap image = null)
        {
            context.StatusCode = (int)code;
            
            Context = context;
            ResponseCode = code;
            Image = image;
        }

        public byte[] GetBytesOfImage()
        {
            byte[] buffer;

            using (var memstream = new MemoryStream())
            {
                Image.Save(memstream, ImageFormat.Png);
                buffer = memstream.GetBuffer();
            }

            return buffer;
        }

        public void Dispose()
        {
            Image?.Dispose();
        }
    }
}