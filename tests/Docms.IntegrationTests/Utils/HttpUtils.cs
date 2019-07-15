using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Docms.IntegrationTests.Utils
{
    class MockHttpServer : IDisposable
    {
        HttpListener _listener;
        Action<HttpListenerContext> _action;
        Task _listeningTask;

        public MockHttpServer(int port, Action<HttpListenerContext> action)
        {
            _action = action;
            _listener = new HttpListener();
            _listener.Prefixes.Add($"http://localhost:{port}/");
            _listener.Start();
            _listeningTask = Task.Run(() =>
            {
                while (_listener.IsListening)
                {
                    var context = _listener.GetContext();
                    _action.Invoke(context);
                }
            });
        }

        public void Dispose()
        {
            _listener.Stop();
            _listener.Close();
        }
    }
}
