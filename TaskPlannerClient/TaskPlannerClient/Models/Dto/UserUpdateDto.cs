using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace TaskPlannerClient.Models.Dto
{
    public class UserUpdateDto
    {
        [JsonProperty("login")]
        public string? Login { get; set; }

        [JsonProperty("password")]
        public string? Password { get; set; }

        [JsonProperty("role")]
        public string? Role { get; set; }

        [JsonProperty("employee_id")]
        public int? EmployeeId { get; set; }

        [JsonProperty("is_active")]
        public bool? IsActive { get; set; }
    }
}
