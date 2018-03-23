using System;
using NLog;

namespace Kontur.ImageTransformer
{
    public static class EntryPoint
    {
        static EntryPoint()
        {
            Logger = LogManager.GetCurrentClassLogger();
        }
        
        public static void Main()
        {
            using (var server = new AsyncHttpServer())
            {
                const string prefix = "http://+:25645/";

                try
                {
                    Logger.Info($"Starting on {prefix}...");
                    server.Start(prefix);
                    Logger.Info("Started");
                }
                catch (Exception e)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Logger.Fatal(e);
                    Console.ForegroundColor = ConsoleColor.White;
                }

                Console.ReadKey(true);
            }
        }

        private static readonly Logger Logger;
    }
}