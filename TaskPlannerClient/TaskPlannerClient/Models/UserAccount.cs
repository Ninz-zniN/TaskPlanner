using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace TaskPlannerClient.Models
{
    public class UserAccount
    {
        [JsonProperty("id_user")]
        public int IdUser { get; set; }

        [JsonProperty("login")]
        public string Login { get; set; }

        [JsonProperty("role")]
        public string Role { get; set; }

        [JsonProperty("is_active")]
        public bool IsActive { get; set; }

        [JsonProperty("employee")]
        public Employee employee { get; set; }
    }

    public class UserAccountListResponse
    {
        [JsonProperty("items")]
        public List<UserAccount> Items { get; set; }

        [JsonProperty("total")]
        public int Total { get; set; }
    }
}
