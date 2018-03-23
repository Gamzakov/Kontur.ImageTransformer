using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Amib.Threading;
using Kontur.ImageTransformer.Interfases;
using NLog;

namespace Kontur.ImageTransformer
{
    internal class AsyncHttpServer : IDisposable
    {
        #region Main

        static AsyncHttpServer()
        {
            Logger = LogManager.GetCurrentClassLogger();
            Logger.Info("Confiration:");
        }

        public AsyncHttpServer()
        {
            _threadPool = new SmartThreadPool(new STPStartInfo { MaxWorkerThreads = 25, MaxQueueLength = 25 });
            _listener = new HttpListener();
            //SetMaxThreads();
            Logger.Info("End Configuration");
            Console.WriteLine();
        }

        public void Start(string prefix)
        {
            lock (_listener)
            {
                if (!_isRunning)
                {
                    _listener.Prefixes.Clear();
                    _listener.Prefixes.Add(prefix);
                    _listener.Start();

                    _listenerThread = new Thread(Listen)
                    {
                        IsBackground = true,
                        Priority = ThreadPriority.Highest
                    };
                    _listenerThread.Start();
                    _isRunning = true;

                    if (!ImageProcessingController.TryInitializeProcessors())
                    {
                        Dispose();
                    }
                }
            }
        }

        public void Stop()
        {
            lock (_listener)
            {
                if (!_isRunning)
                    return;

                _listener.Stop();

                _listenerThread.Abort();
                _listenerThread.Join();

                _isRunning = false;
            }
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;

            Stop();

            _listener.Close();
        }

        private void Listen()
        {
            while (true)
            {
                try
                {
                    if (_listener.IsListening)
                    {
                        var context = _listener.GetContext();

                        if (_threadPool.MaxQueueLength == _threadPool.CurrentWorkItemsCount)
                        {
                            context.Response.StatusCode = (int)HttpStatusCode.ServiceUnavailable;
                            context.Response.Close();
                        }
                        else
                            _threadPool.QueueWorkItem(HandleContext, context);


                        // if (length > 0)
                        // {
                        //ThreadPool.GetAvailableThreads(out var workers, out _);
                        //if (workers == 0)
                        //{
                        //    context.Response.StatusCode = (int)HttpStatusCode.ServiceUnavailable;
                        //    context.Response.Close();
                        //}
                        //else
                        //{
                        //    Logger.Info($"Number of free worker: {workers}");
                        //    ThreadPool.QueueUserWorkItem(HandleContext, context);
                        //}
                        // }
                        // else
                        // {
                        //   context.Response.StatusCode = (int) HttpStatusCode.NoContent;
                        //   context.Response.Close();
                        //   Logger.Info("Отсутствует тело запроса");
                        // }
                    }
                    else
                        Thread.Sleep(0);
                }
                catch (ThreadAbortException)
                {
                    return;
                }
                catch (Exception error)
                {
                    Logger.Fatal(error);
                }
            }
        }

//        private void SetMaxThreads()
//        {
//            if (ThreadPool.SetMaxThreads(Environment.ProcessorCount * 3, Environment.ProcessorCount * 3))
//            {
//                ThreadPool.GetMaxThreads(out var workers, out var ports);
//                Logger.Info($"Max threads number is {workers} and their ports number is {ports}");
//            }
//            else
//            {
//                Logger.Warn("Max threads не установлено");
//            }
//        }

        #endregion

        private static async void HandleContext(object listenerContext)
        {
            var request = new Request((HttpListenerContext)listenerContext);
            var response = await request.Process(request);

            await SendResponse(response);
        }

        private static async Task SendResponse(IResponse response)
        {
            try
            {
                if (response.Image == null)
                {
                    response.Context.Close();
                    return;
                }
            }
            catch
            {
                Logger.Warn("Connection was close before we manually close it with no success status code");
                return;
            }

            Stream stream = null;

            try
            {
                stream = response.Context.OutputStream;
                var buffer = response.GetBytesOfImage();
                await stream.WriteAsync(buffer, 0, buffer.Length);
            }
            catch
            {
                Logger.Warn("Connection was close before we write image to stream");
            }
            finally
            {
                stream?.Dispose();
            }
        }

        #region Fields

        private readonly SmartThreadPool _threadPool;
        private readonly HttpListener _listener;
        private static readonly Logger Logger;
        private volatile bool _isRunning;
        private Thread _listenerThread;
        private bool _disposed;

        #endregion
    }
}