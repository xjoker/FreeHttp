//#define NET4_5UP

using System;
using System.Net;
using System.Text;

namespace FreeHttp.WebService.HttpServer
{
    public class MyHttpListener
    {
        private readonly HttpListener listener;

        public MyHttpListener()
        {
            if (!HttpListener.IsSupported) return;
            listener = new HttpListener();
        }

        public bool IsStart => listener == null ? false : listener.IsListening;

        public event EventHandler<HttpListenerMessageEventArgs> OnGetHttpListenerMessage;

        public bool Start(string prefixes)
        {
            return Start(new[] { prefixes }, true);
        }

        public bool Start(string[] prefixesArray, bool isClear)
        {
            if (!HttpListener.IsSupported) throw new Exception("not supported");

            //listener.Prefixes.Add("http://localhost:9998/");
            //listener.Prefixes.Add("https://localhost:44399/");
            //listener.Prefixes.Add("https://*:443/");
            //listener.Prefixes.Add("https://*:9996/");
            //listener.Prefixes.Add("https://*:9996/test/");
            try
            {
                if (isClear) listener.Prefixes.Clear();
                foreach (var prefixes in prefixesArray) listener.Prefixes.Add(prefixes);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            try
            {
                if (!listener.IsListening)
                    listener.Start();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            ListenerAsync();
            return true;

        }

        public void Close()
        {
            if (listener != null)
            {
                Stop();
                listener.Close();
            }
        }

        public void Stop()
        {
            if (listener != null && listener.IsListening) listener.Stop();
        }

        private async void ListenerAsync()
        {
            HttpListenerContext context;
            var responseString = "Hello FreeHttp";
            var buffer = Encoding.UTF8.GetBytes(responseString);
            while (listener.IsListening)
                try
                {
                    context = await listener.GetContextAsync();
                    var request = context.Request;
                    var response = context.Response;
                    response.ContentLength64 = buffer.Length;
                    var output = response.OutputStream;
                    await output.WriteAsync(buffer, 0, buffer.Length);
                    output.Close();
                }
                catch (Exception ex)
                {
                    if (!IsStart)
                    {
                        return;
                    }

                    if (OnGetHttpListenerMessage != null)
                        OnGetHttpListenerMessage(this, new HttpListenerMessageEventArgs(true, ex.Message));
                }
        }

        private void ListenerWorker()
        {
            while (listener.IsListening)
            {
                // Note: The GetContext method blocks while waiting for a request. 
                var context = listener.GetContext();
                var request = context.Request;
                // Obtain a response object.
                var response = context.Response;
                // Construct a response.
                var responseString = "ok";
                var buffer = Encoding.UTF8.GetBytes(responseString);
                // Get a response stream and write the response to it.
                response.ContentLength64 = buffer.Length;
                var output = response.OutputStream;
                output.Write(buffer, 0, buffer.Length);
                // You must close the output stream.
                output.Close();
                //listener.Stop();
            }
        }

        public class HttpListenerMessageEventArgs : EventArgs
        {
            public HttpListenerMessageEventArgs(bool isErrorMessage, string message)
            {
                IsErrorMessage = isErrorMessage;
                Message = message;
            }

            public bool IsErrorMessage { get; set; }
            public string Message { get; set; }
        }
    }
}