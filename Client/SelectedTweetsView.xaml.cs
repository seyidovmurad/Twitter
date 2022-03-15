using Server.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Client
{
    /// <summary>
    /// Interaction logic for SelectedTweetsView.xaml
    /// </summary>
    public partial class SelectedTweetsView : Window
    {

        public User User { get; set; }
        public SelectedTweetsView(User user)
        {
            InitializeComponent();
            User = user;
            TweetListBox.Visibility = user.Tweets?.Count == 0 ? Visibility.Hidden : Visibility.Visible;
            DataContext = this;
        }
    }
}
