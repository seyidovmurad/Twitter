using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Server.Data;
using Server.Models;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

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

        private static async Task Get(HttpListenerContext context)
        {
            var request = context.Request;
            var response = context.Response;
            var username = request.QueryString["username"] ?? "all";
            var search = request.QueryString["search"] ?? "";
            var isLogin = bool.Parse(request.QueryString["login"] ?? "false");
            HttpResponseMessage res = null;
            try
            {
                res = await client.GetAsync($"{uri}{request.RawUrl}");
            }
            catch { }
            var sw = new StreamWriter(response.OutputStream);

            if (res == null || res.StatusCode == HttpStatusCode.NotFound)
            {
                if (username == "all")
                {
                    var users = DbContext.User.Where(u => u.Name.Contains(search) || u.Username.Contains(search) || u.Surname.Contains(search)).ToList();
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
                    var user = DbContext.User.AsNoTracking().Include(u => u.Tweets).FirstOrDefault(u => u.Username == username);
                    if (user == null)
                    {
                        response.StatusCode = (int)HttpStatusCode.NotFound;
                        return;
                    }
                    if (!isLogin)
                    {
                        IncreaseViewCount(user);
                        await TrySaveCasheServer(user);
                    }
                    sw.Write(JsonConvert.SerializeObject(user));
                    sw.Close();
                }
            }
            else if (res.StatusCode == HttpStatusCode.OK)
            {
                var str = await res.Content.ReadAsStringAsync();

                if (username != "all")
                {
                    var user = JsonConvert.DeserializeObject<User>(str);
                    if (!isLogin)
                    {
                        IncreaseViewCount(user);
                        await TrySaveCasheServer(user);
                    }
                }
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
            var title = req.QueryString["title"];
            var username = req.QueryString["username"] ?? "";
            var sr = new StreamReader(req.InputStream);
            var json = sr.ReadToEnd();
            if (title == "user")
            {
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
            else if(title == "tweet")
            {
                if (string.IsNullOrEmpty(username))
                {
                    context.Response.StatusCode = (int)HttpStatusCode.NoContent;
                    return;
                }

                var user = DbContext.User.FirstOrDefault(u => u.Username == username);

                if(user == null)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    return;
                }

                Tweet tweet = null;
                try
                {
                    tweet = JsonConvert.DeserializeObject<Tweet>(json);
                }
                catch
                {
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return;
                }

                user.Tweets.Add(tweet);

                try
                {
                    DbContext.Update(user);
                    DbContext.SaveChanges();
                    client.PostAsync($"{uri}?username={username}&title=tweet", new StringContent(json)).Wait();
                }
                catch 
                {
                }
            }
            else
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return;
            }
        }
        private static void Delete(HttpListenerContext context)
        {

        }

        private static void IncreaseViewCount(User user)
        {
            try
            {
                user.ViewCount++;
                DbContext.Update(user);
                DbContext.SaveChanges();
            }
            catch { }
        }

        private async static Task TrySaveCasheServer(User user)
        {
            if(user.ViewCount > 6)
            {
                var json = JsonConvert.SerializeObject(user);
                try
                {
                    await client.PostAsync(uri, new StringContent(json));
                }
                catch { }
            }
        }

    }
}
