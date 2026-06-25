using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace TaskPlannerClient.Models
{
    public class Employee
    {
        [JsonProperty("id_employee")]
        public int IdEmployee { get; set; }

        [JsonProperty("last_name")]
        public string LastName { get; set; }

        [JsonProperty("first_name")]
        public string FirstName { get; set; }

        [JsonProperty("patronymic")]
        public string Patronymic { get; set; }

        [JsonProperty("position")]
        public string Position { get; set; }

        [JsonProperty("grade")]
        public string Grade { get; set; }

        [JsonProperty("hours_per_day")]
        public decimal HoursPerDay { get; set; }

        [JsonProperty("is_dismissed")]
        public bool IsDismissed { get; set; }

        [JsonProperty("team")]
        public Team Team { get; set; }
    }

    public class Team
    {
        [JsonProperty("id_team")]
        public int IdTeam { get; set; }

        [JsonProperty("team_name")]
        public string TeamName { get; set; }

        [JsonProperty("team_lead")]
        public Employee TeamLead { get; set; }
    }

    public class EmployeeListResponse
    {
        [JsonProperty("items")]
        public List<Employee> Items { get; set; }

        [JsonProperty("total")]
        public int Total { get; set; }
    }
}
