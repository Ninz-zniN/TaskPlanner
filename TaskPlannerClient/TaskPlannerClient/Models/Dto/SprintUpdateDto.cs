using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using TaskPlannerClient.Converters;

namespace TaskPlannerClient.Models.Dto
{
    public class SprintUpdateDto
    {
        [JsonProperty("sprint_name")]
        public string? SprintName { get; set; }

        [JsonProperty("start_date")]
        [JsonConverter(typeof(LocalDateConverter))]
        public DateTime? StartDate { get; set; }

        [JsonProperty("end_date")]
        [JsonConverter(typeof(LocalDateConverter))]
        public DateTime? EndDate { get; set; }

        [JsonProperty("work_days")]
        public int? WorkDays { get; set; }
    }
}
