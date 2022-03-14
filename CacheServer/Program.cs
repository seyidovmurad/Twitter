using Server;
using System;
using System.Net;

namespace CacheServer
{
    class Program
    {
        static Hub Hub = new Hub("http://localhost:5000/", new HttpListener());
        static void Main(string[] args)
        {
            Hub.Get += Get;
            Hub.Put += Put;
            Hub.Post += Post;
            Hub.Delete += Delete;
            Hub.Start();
            
        }

        static void Get(HttpListenerContext context)
        {
            var res = context.Response;
            res.StatusCode = (int)HttpStatusCode.NotFound;
        }

        private static void Put(HttpListenerContext context)
        {

        }
        private static void Post(HttpListenerContext context)
        {

        }
        private static void Delete(HttpListenerContext context)
        {

        }
    }
}
