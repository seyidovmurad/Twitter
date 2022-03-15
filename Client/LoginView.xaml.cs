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
using System.Windows.Threading;

namespace Client
{
    /// <summary>
    /// Interaction logic for LoginView.xaml
    /// </summary>
    public partial class LoginView : Page
    {
        private HttpClient client;

        public string Uri { get; set; }
        public LoginView(HttpClient client)
        {
            InitializeComponent();
            this.client = client;
        }

        private async void LoginBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(UsernameTxtb.Text) && !string.IsNullOrEmpty(PasswordTxtb.Password))
            {
                try
                {
                    var uri = $"{Uri}?username={UsernameTxtb.Text}&login=true";
                        var res = await client.GetAsync(uri);

                        if (res.IsSuccessStatusCode)
                        {
                            var json = await res.Content.ReadAsStringAsync();
                            var user = JsonConvert.DeserializeObject<User>(json);

                            if (user.Password == PasswordTxtb.Password)
                            {
                                this.NavigationService.Navigate(new UsersView(client, user) { Uri = Uri });
                            }
                            else
                            {
                                MessageBox.Show("Wrong Password");
                            }
                        }
                        else if (res.StatusCode == System.Net.HttpStatusCode.NotFound)
                        {
                            MessageBox.Show("Username not found");
                        }
                        else
                        {
                            MessageBox.Show("Something went wrong :(");
                        }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else
            {
                MessageBox.Show("Enter Username and Password");
            }
        }

        private async void SignUpBtn_Click(object sender, RoutedEventArgs e)
        {
            if(!string.IsNullOrEmpty(NameTxtb.Text) && !string.IsNullOrEmpty(SurnameTxtb.Text) && !string.IsNullOrEmpty(UsernameTxtb2.Text) && PasswordTxtb2.Password.Length > 8)
            {
                try
                {
                    var user = new User() { Name = NameTxtb.Text, Surname = SurnameTxtb.Text, Username = UsernameTxtb2.Text, Password = PasswordTxtb2.Password };
                    using (var res = await client.PostAsync($"{Uri}?title=user", new StringContent(JsonConvert.SerializeObject(user))))
                    {
                        if(res.IsSuccessStatusCode)
                        {
                             this.NavigationService.Navigate(new UsersView(client, user) { Uri = Uri });
                        }
                        else if(res.StatusCode == System.Net.HttpStatusCode.Conflict)
                        {
                            MessageBox.Show("This username already exist");
                        }
                        else
                        {
                            MessageBox.Show("Something went wrong");
                        }
                    }
                    
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else
            {
                if (PasswordTxtb2.Password.Length <= 8)
                    MessageBox.Show("Min password length 8");
                else
                    MessageBox.Show("Fill all field");
            }
        }

    }
}
