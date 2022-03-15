using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Server.Models
{
    public class Tweet: Entity
    {
        public string Content { get; set; }

        public int Likes { get; set; }

        public int UserId { get; set; }

        [JsonIgnore]
        [IgnoreDataMember]
        public User User { get; set; }
        
    }
}
