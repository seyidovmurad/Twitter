using Newtonsoft.Json;
using Server;
using Server.Models;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Net;

namespace CacheServer
{
    class Program
    {
        static ConcurrentBag<User> Users = new ConcurrentBag<User>();

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
            var request = context.Request;
            var response = context.Response;
            var username = request.QueryString["username"] ?? "all";
            var search = request.QueryString["search"] ?? "";
            var isLogin = bool.Parse(request.QueryString["login"] ?? "false");
            var sw = new StreamWriter(response.OutputStream);

            if (username == "all")
            {
                var users = Users.Where(u => u.Name.Contains(search) || u.Username.Contains(search) || u.Surname.Contains(search)).ToList();
                if (users.Count == 0)
                {
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    return;
                }

                sw.Write(JsonConvert.SerializeObject(users));
                sw.Close();
            }
            else
            {
                var user = Users.FirstOrDefault(u => u.Username == username);
                if (user == null)
                {
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    return;
                }
                sw.Write(JsonConvert.SerializeObject(user));
                sw.Close();
            }
        }

        private static void Put(HttpListenerContext context)
        {

        }
        private static void Post(HttpListenerContext context)
        {
            var req = context.Request;
            var title = req.QueryString["title"] ?? "user";
            var username = req.QueryString["username"] ?? "";
            var sr = new StreamReader(req.InputStream);
            var json = sr.ReadToEnd();

            if (title == "tweet")
            {
                var tweet = JsonConvert.DeserializeObject<Tweet>(json);

                var user = Users.FirstOrDefault(u => u.Username == username);

                if (user != null)
                {
                    user.Tweets.Add(tweet);
                }
            }
            else
            {
                var user = JsonConvert.DeserializeObject<User>(json);

                if (!Users.Contains(user))
                {
                    Users.Add(user);
                }
            }
        }
        private static void Delete(HttpListenerContext context)
        {

        }
    }
}
