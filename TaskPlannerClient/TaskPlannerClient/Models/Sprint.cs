using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using TaskPlannerClient.Converters;

namespace TaskPlannerClient.Models
{
    public class Sprint
    {
        [JsonProperty("id_sprint")]
        public int IdSprint { get; set; }

        [JsonProperty("sprint_name")]
        public string SprintName { get; set; }

        [JsonProperty("start_date")]
        [JsonConverter(typeof(LocalDateConverter))]
        public DateTime StartDate { get; set; }

        [JsonProperty("end_date")]
        [JsonConverter(typeof(LocalDateConverter))]
        public DateTime EndDate { get; set; }

        [JsonProperty("work_days")]
        public int WorkDays { get; set; }

        [JsonProperty("is_active")]
        public bool IsActive { get; set; }
    }

    public class SprintListResponse
    {
        [JsonProperty("items")]
        public List<Sprint> Items { get; set; }

        [JsonProperty("total")]
        public int Total { get; set; }
    }
}
