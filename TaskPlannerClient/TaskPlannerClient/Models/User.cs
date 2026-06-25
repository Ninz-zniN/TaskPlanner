using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace TaskPlannerClient.Models
{
    public class User
    {
        [JsonProperty("id_user")]
        public int IdUser { get; set; }

        [JsonProperty("login")]
        public string Login { get; set; }

        [JsonProperty("role")]
        public string Role { get; set; }

        [JsonProperty("employee")]
        public Employee Employee { get; set; }
    }

    public class LoginResponse
    {
        [JsonProperty("token")]
        public string Token { get; set; }

        [JsonProperty("user")]
        public User User { get; set; }
    }

    public class LoginRequest
    {
        [JsonProperty("login")]
        public string Login { get; set; }

        [JsonProperty("password")]
        public string Password { get; set; }
    }
}
