using System.Collections.Generic;

namespace ZeroKey.Service.Common
{
    public class Config
    {
        public List<User> Users { get; set; }
    }

    public class User
    {
        public string Username { get; set; }
        public Dictionary<string, string> Tokens { get; set; }
        public List<Event> Events { get; set; }
        public string Salt { get; set; }
    }

    public class Event
    {
        public string Token { get; set; }
        public string PluginName { get; set; }
        public Dictionary<string, object> Parameters { get; set; }
    }
}
