using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using TaskPlannerClient.Converters;

namespace TaskPlannerClient.Models.Dto
{
    public class CreateSprintDto
    {
        [JsonProperty("sprint_name")]
        public string SprintName { get; }

        [JsonProperty("start_date")]
        [JsonConverter(typeof(LocalDateConverter))]
        public DateTime StartDate { get; }

        [JsonProperty("end_date")]
        [JsonConverter(typeof(LocalDateConverter))]
        public DateTime EndDate { get; }

        [JsonProperty("work_days")]
        public int WorkDays { get; }

        public CreateSprintDto(string sprintName, DateTime startDate, DateTime endDate, int workDays)
        {
            SprintName = sprintName;
            StartDate = startDate;
            EndDate = endDate;
            WorkDays = workDays;
        }
    }
}
