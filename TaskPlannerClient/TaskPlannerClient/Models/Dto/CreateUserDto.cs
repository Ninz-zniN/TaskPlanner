using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace TaskPlannerClient.Models.Dto
{
    public class CreateUserDto
    {
        [JsonProperty("login")]
        public string Login { get; }

        [JsonProperty("password")]
        public string Password { get; }

        [JsonProperty("role")]
        public string Role { get; }

        [JsonProperty("employee_id")]
        public int? EmployeeId { get; }

        public CreateUserDto(string login, string password, string role = "worker", int? employeeId = null)
        {
            Login = login;
            Password = password;
            Role = role;
            EmployeeId = employeeId;
        }
    }
}
