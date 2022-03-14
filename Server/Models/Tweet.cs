using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Models
{
    public class Tweet: Entity
    {
        public string Content { get; set; }

        public int Likes { get; set; }

        public int UserId { get; set; }

        public User User { get; set; }
        
    }
}
