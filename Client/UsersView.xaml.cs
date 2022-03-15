using Newtonsoft.Json;
using PropertyChanged;
using Server.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    [AddINotifyPropertyChangedInterface]
    public partial class UsersView : Page
    {
        private HttpClient client;

        public User User { get; set; }

        public string Uri { get; set; }

        public ObservableCollection<Tweet> Tweets { get; set; }

        public ObservableCollection<User> Users { get; set; }

        public UsersView(HttpClient client, User user)
        {
            InitializeComponent();
            DataContext = this;
            this.client = client;
            User = user;

            Tweets = new ObservableCollection<Tweet>(user.Tweets);

            NoTweet.Visibility = Visibility.Hidden;
            NotUser.Visibility = Visibility.Hidden;
            IsHidden = Tweets.Count == 0;
        }

        private bool isHidden;

        public bool IsHidden
        {
            get => isHidden;
            set
            {
                isHidden = value;
                if (isHidden)
                {
                    NoTweet.Visibility = Visibility.Visible;
                    TweetListBox.Visibility = Visibility.Hidden;
                }
                else
                {
                    NoTweet.Visibility = Visibility.Hidden;
                    TweetListBox.Visibility = Visibility.Visible;
                }
            }
        }

        //post tweet
        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            var window = new PostView();
            if (window.ShowDialog() == true)
            {
                var rand = new Random();
                var tweet = new Tweet
                {
                    Content = window.Text,
                    Likes = rand.Next(0, 300)
                };


                IsHidden = false;
                try
                {
                    var res = await client.PostAsync($"{Uri}?title=tweet&username={User.Username}", new StringContent(JsonConvert.SerializeObject(tweet)));

                    if(res.IsSuccessStatusCode)
                        Tweets.Add(tweet);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        //search
        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var res = await client.GetAsync($"{Uri}?search={SearchTxtb.Text}");

            if (res.IsSuccessStatusCode)
            {
                var json = await res.Content.ReadAsStringAsync();
                var users = JsonConvert.DeserializeObject<List<User>>(json);

                Users = new ObservableCollection<User>(users);
                NotUser.Visibility = Visibility.Hidden;
                UsersListb.Visibility = Visibility.Visible;
            }
            else if (res.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                MessageBox.Show("Not found");
                UsersListb.Visibility = Visibility.Hidden;
                NotUser.Visibility = Visibility.Visible;
            }
        }

        //double click
        private async void UsersListb_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var user = UsersListb.SelectedItem as User;

            var res = await client.GetAsync($"{Uri}?username={user.Username}");

            if(res.IsSuccessStatusCode)
            {
                var json = await res.Content.ReadAsStringAsync();

                var window = new SelectedTweetsView(JsonConvert.DeserializeObject<User>(json));
                window.Show();
            }
            else
            {
                MessageBox.Show("Someting went wrong :(");
            }

        }
    }
}
