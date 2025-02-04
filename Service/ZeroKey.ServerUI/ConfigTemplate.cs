﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZeroKey.ServerUI
{
    public class ConfigTemplate
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
    
    public class MedatixxUser
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Card { get; set; }
    }
}
