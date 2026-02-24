using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Diary_API.Data
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }
        public bool IsBlocked { get; set; } = false;

        [JsonIgnore]
        public List<Entry> Entries { get; set; } = new List<Entry>();
    }
}
