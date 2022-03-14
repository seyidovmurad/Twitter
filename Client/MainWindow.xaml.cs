using Newtonsoft.Json;
using Server.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public HttpClient client = new HttpClient();

        public string uri = "http://localhost:2700/";
        public MainWindow()
        {
            InitializeComponent();
            GetUser();
        }

        public void GetUser()
        {
            var user = new User { Name = "Murad", Password = "Murad123", Username = "fadfrad", ViewCount = 5, Surname = "Seyidov" };

            try
            {
                var response = client.PutAsync(uri, new StringContent(JsonConvert.SerializeObject(user))).Result;
                txt.Text = response.StatusCode.ToString();
            }
            catch
            {
                txt.Text = "No Network";
            }
            //var res = client.GetAsync($"{uri}?id=3").GetAwaiter().GetResult();

            //if(res.StatusCode == System.Net.HttpStatusCode.OK)
            //{
            //    var json = res.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            //    var user = JsonConvert.DeserializeObject<User>(json);
            //    if (user != null)
            //        txt.Text = user.ToString();
            //    else
            //        txt.Text = "null";
            //}
            //else
            //{
            //    txt.Text = res.StatusCode.ToString();
            //}
        }
    }
}
