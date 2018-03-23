using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace Client
{
    internal static class Program
    {
        static Program()
        {
            var image = Image.FromFile(Directory.GetCurrentDirectory() + "\\image.png");

            using (var memstream = new MemoryStream())
            {
                image.Save(memstream, ImageFormat.Png);
                Buffer = memstream.GetBuffer();
            }
        }

        private static void Main()
        {
            Console.Write("IP: ");
            var ip = Console.ReadLine();
            Console.Write("Преобразование(rotate-cw|rotate-ccw|flip-h|flip-v): ");
            var filter = Console.ReadLine();
            Console.Write("Координаты (10,10,10,10): ");
            var coords = Console.ReadLine();
            Console.Write("Кол-во запросов: ");
            var x = Convert.ToInt32(Console.ReadLine());
            Console.WriteLine("Sending ...");

            
            Parallel.For(0, x, async i => await SendAsync(i, filter, ip, coords));

            Console.ReadKey();
        }

        private static async Task SendAsync(int i, string filter, string ip, string coords)
        {
            Configurate(out var request, filter, ip, coords);
            await Send(i, request);
        }

        private static void Configurate(out HttpWebRequest request, string filter, string ip, string coords)
        {
            request = WebRequest.CreateHttp($"http://{ip}:25645/process/{filter}/{coords}");
            request.Method = WebRequestMethods.Http.Post;
            request.ContentType = "application/octet-stream";
            request.ContentLength = Buffer.Length;
        }

        private static async Task Send(int i, WebRequest request)
        {
            using (var reqstream = request.GetRequestStream())
            {
                await reqstream.WriteAsync(Buffer, 0, Buffer.Length);
                Console.WriteLine("Sended!");
            }

            Console.WriteLine("Accepting...");
            try
            {
                if (await request.GetResponseAsync() is HttpWebResponse response)
                {
                    if (response.StatusCode == HttpStatusCode.BadRequest)
                    {
                        Console.WriteLine("httpStatusCode: " + HttpStatusCode.BadRequest);
                        return;
                    }

                    Console.WriteLine("httpStatusCode: " + response.StatusCode);

                    var resstream = response.GetResponseStream();

                    if (resstream != null)
                    {
                        var image = Image.FromStream(resstream);
                        image.Save(Directory.GetCurrentDirectory() + $"\\Accepted\\result{i}.png", ImageFormat.Png);
                        Console.WriteLine("Accepted!");
                    }
                    else
                    {
                        throw new Exception("ResponseStream is empty!");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: " + ex.Message);
            }
        }

        private static readonly byte[] Buffer;
    }
}