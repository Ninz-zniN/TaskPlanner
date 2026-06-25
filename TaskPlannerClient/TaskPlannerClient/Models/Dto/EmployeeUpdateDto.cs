using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace TaskPlannerClient.Models.Dto
{
    public class EmployeeUpdateDto
    {
        [JsonProperty("last_name")]
        public string? LastName { get; set; }

        [JsonProperty("first_name")]
        public string? FirstName { get; set; }

        [JsonProperty("patronymic")]
        public string? Patronymic { get; set; }

        [JsonProperty("position")]
        public string? Position { get; set; }

        [JsonProperty("grade")]
        public string? Grade { get; set; }

        [JsonProperty("id_team")]
        public int? IdTeam { get; set; }

        [JsonProperty("hours_per_day")]
        public decimal? HoursPerDay { get; set; }

        [JsonProperty("is_dismissed")]
        public bool? IsDismissed { get; set; }
    }
}
