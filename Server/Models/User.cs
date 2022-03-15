using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Models
{
    public class User : Entity
    {
        public string Username { get; set; }

        public string Password { get; set; }

        public string Name { get; set; }

        public string Surname { get; set; }

        public int ViewCount { get; set; }

        public List<Tweet> Tweets { get; set; }

        public User()
        {
            Tweets = new();
        }
    }
}
