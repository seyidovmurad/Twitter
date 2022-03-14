using Server.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    class Hub
    {
        private HttpListener listener;

        private string uri;

        public HttpListenerContext Context { get; set; }

        public Action<HttpListenerContext> Get;

        public Action<HttpListenerContext> Post;

        public Action<HttpListenerContext> Delete;

        public Action<HttpListenerContext> Put;

        public Hub(string uri,HttpListener listener)
        {
            this.uri = uri;
            this.listener = listener;
        }

        public void Start()
        {
            listener.Prefixes.Add(uri);
            listener.Start();
            Task.Run(() =>
            {
                while (true)
                {
                    Context = listener.GetContext();

                    var response = Context.Response;
                    var request = Context.Request;

                    if (request.HttpMethod == HttpMethod.Post.Method)
                        Post?.Invoke(Context);
                    else if (request.HttpMethod == HttpMethod.Get.Method)
                        Get?.Invoke(Context);
                    else if (request.HttpMethod == HttpMethod.Delete.Method)
                        Delete?.Invoke(Context);
                    else if (request.HttpMethod == HttpMethod.Put.Method)
                        Put?.Invoke(Context);
                    response.Close();
                }
            }).Wait();
        }
    }
}
