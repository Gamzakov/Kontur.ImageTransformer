using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Kontur.ImageTransformer.Enums;
using Kontur.ImageTransformer.Interfases;
using NLog;

namespace Kontur.ImageTransformer
{
    public sealed class Request : BaseRequest
    {
        public override HttpListenerContext Context { get; }
        public override string Filter { get; }
        public override int[] Coords { get; }
        public override Bitmap Image { get; }
        public override bool IsValidated { get; }

        static Request()
        {
            Logger = LogManager.GetCurrentClassLogger();
        }

        public Request(HttpListenerContext context)
        {
            var url = GetUrl(context);
            _urlIsValid = UrlIsValid(url);

            Coords = GetCoords(url);
            Filter = GetFilter(url);
            Image = GetImage(context.Request);
            Context = context;

            IsValidated = IsValid();
        }

        public override async Task<IResponse> Process(IRequest request)
        {
            if (_urlIsValid)
            {
                if (IsValid())
                {
                    var image = ImageProcessingController.ApplyFilter(Image, Filter, Coords);

                    if (image == null)
                        return new Response(Context.Response, HttpStatusCode.NoContent);

                    return new Response(Context.Response, HttpStatusCode.OK, image);
                }
                
                return new Response(Context.Response, HttpStatusCode.NoContent);
            }

            return new Response(Context.Response, HttpStatusCode.BadRequest);
        }

        protected override Bitmap GetImage(HttpListenerRequest request)
        {
            Image image = null;
            Stream stream = null;
            
            try
            {
                stream = request.InputStream;
                image = System.Drawing.Image.FromStream(stream);
            }
            catch
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Logger.Warn("Не удалось извлечь изображение из тела запроса");
                Console.ForegroundColor = ConsoleColor.White;
            }
            finally
            {
                stream?.Dispose();
            }

            return (Bitmap)image;
        }

        protected override Uri GetUrl(HttpListenerContext context)
        {
            return context.Request.Url;
        }

        protected override string GetFilter(Uri url)
        {
            return url.Segments[2].ToLowerInvariant().Replace("/", "").Replace("-", "");
        }

        protected override bool UrlIsValid(Uri url)
        {
            var filterIsValid = false;
            var segmentsCount = url.Segments.Length;
            var segmentsCountIsValid = segmentsCount == 4;
            
            if (segmentsCountIsValid)
            {
                var filter = GetFilter(url);
                var filters = Enum.GetNames(typeof(Filters)).Select(x => x.ToLowerInvariant());
                filterIsValid = filters.Contains(filter);
            }

            var result = segmentsCountIsValid && url.Segments[1].ToLowerInvariant() == "process/" && filterIsValid;

            return result;
        }

        protected override int[] GetCoords(Uri url)
        {
            if (!_urlIsValid)
                return null;

            var rawCoords = url.Segments[3];
            var coords = rawCoords.Split(',') // coords[0] = x, coords[1] = y, coords[2] = w, coords[3] = h
                .Where(s => int.TryParse(s, out _))
                .Select(int.Parse).ToArray();


            return coords.Length == 4 ? coords : null;
        }

        protected override bool IsValid()
        {
            var coordsIsValid = Coords != null && Coords.Length == 4;

            var result = coordsIsValid && Image != null;

            return result;
        }

        public override void Dispose()
        {
            Image?.Dispose();
        }

        private static readonly Logger Logger;
        private readonly bool _urlIsValid;
    }
}