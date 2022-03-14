using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Server.Data;
using Server.Models;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;

namespace Server
{
    class Program
    {
        static Hub Hub = new Hub("http://localhost:2700/", new HttpListener());

        static AppDbContext DbContext = new AppDbContext();

        static HttpClient client = new HttpClient();

        static string uri = "http://localhost:5000/";
        static void Main(string[] args)
        {
            Hub.Get += Get;
            Hub.Put += Put;
            Hub.Post += Post;
            Hub.Delete += Delete;
            Hub.Start();
        }

        private static void Get(HttpListenerContext context)
        {
            var request = context.Request;
            var response = context.Response;
            var id = request.QueryString["id"];
            var res = client.GetAsync($"{uri}?id={id}").GetAwaiter().GetResult();
            var sw = new StreamWriter(response.OutputStream);

            if (res.StatusCode == HttpStatusCode.NotFound)
            {
                if (int.TryParse(id, out int result))
                {
                    if(result == -1)
                    {
                        var users = DbContext.User.Include(u => u.Tweets).ToList();
                        if(users == null)
                        {
                            response.StatusCode = (int)HttpStatusCode.NotFound;
                            return;
                        }

                        sw.Write(JsonConvert.SerializeObject(users));
                        sw.Close();
                    }
                    else
                    {
                        var user = DbContext.User.Include(u => u.Tweets).FirstOrDefault(u => u.Id == result);
                        if (user == null)
                        {
                            response.StatusCode = (int)HttpStatusCode.NotFound;
                            return;
                        }
                        IncreaseViewCount(user);
                        TrySaveCasheServer(user);
                        sw.Write(JsonConvert.SerializeObject(user));
                        sw.Close();
                    }
                }
                else
                {
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return;
                }
            }
            else if(res.StatusCode == HttpStatusCode.OK)
            {
                var str = res.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                var user = JsonConvert.DeserializeObject<User>(str);
                IncreaseViewCount(user);
                TrySaveCasheServer(user);
                sw.Write(str);
            }
            else
            {
                response.StatusCode = (int)res.StatusCode;
                return;
            }

            response.StatusCode = (int)HttpStatusCode.OK;
        }
        private static void Put(HttpListenerContext context)
        {
            var req = context.Request;
            var sr = new StreamReader(req.InputStream);
            var json = sr.ReadToEnd();

            User user = null;
            try
            {
                user = JsonConvert.DeserializeObject<User>(json);
            }
            catch
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return;
            }

            if (user != null)
            {
                var usr = DbContext.User.FirstOrDefault(u => u.Username == user.Username);
                usr.ViewCount = user.ViewCount;
                try
                {
                    DbContext.Update(usr);
                    DbContext.SaveChanges();
                    context.Response.StatusCode = (int)HttpStatusCode.OK;
                }
                catch
                {
                    context.Response.StatusCode = (int)HttpStatusCode.Conflict;
                    return;
                }
            }
            else
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            }
        }
        private static void Post(HttpListenerContext context)
        {
            var req = context.Request;
            var sr = new StreamReader(req.InputStream);
            var json = sr.ReadToEnd();

            User user = null;
            try
            {
                user = JsonConvert.DeserializeObject<User>(json);
            }
            catch
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return;
            }

            if (user != null)
            {
                try
                {
                    DbContext.Add(user);
                    DbContext.SaveChanges();
                    context.Response.StatusCode = (int)HttpStatusCode.OK;
                }
                catch
                {
                    context.Response.StatusCode = (int)HttpStatusCode.Conflict;
                }
            }
            else
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            }
        }
        private static void Delete(HttpListenerContext context)
        {

        }

        private static void IncreaseViewCount(User user)
        {
            user.ViewCount++;
            DbContext.Update(user);
            DbContext.SaveChanges();
        }

        private static void TrySaveCasheServer(User user)
        {
            if(user.ViewCount > 6)
            {
                var json = JsonConvert.SerializeObject(user);
                client.PostAsync(uri, new StringContent(json));
            }
        }

    }
}
