using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace TaskPlannerClient.Models.Dto
{
    public class CreateEmployeeDto
    {
        [JsonProperty("last_name")]
        public string LastName { get; }

        [JsonProperty("first_name")]
        public string FirstName { get; }

        [JsonProperty("patronymic")]
        public string? Patronymic { get; }

        [JsonProperty("position")]
        public string? Position { get; }

        [JsonProperty("grade")]
        public string? Grade { get; }

        [JsonProperty("id_team")]
        public int? IdTeam { get; }

        [JsonProperty("hours_per_day")]
        public decimal HoursPerDay { get; }

        public CreateEmployeeDto(
            string lastName,
            string firstName,
            string? patronymic = null,
            string? position = null,
            string? grade = null,
            int? idTeam = null,
            decimal hoursPerDay = 8)
        {
            LastName = lastName;
            FirstName = firstName;
            Patronymic = patronymic;
            Position = position;
            Grade = grade;
            IdTeam = idTeam;
            HoursPerDay = hoursPerDay;
        }
    }
}
