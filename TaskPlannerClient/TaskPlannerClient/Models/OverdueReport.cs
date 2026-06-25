using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using TaskPlannerClient.Converters;

namespace TaskPlannerClient.Models
{
    public class OverdueReport
    {
        [JsonProperty("items")]
        public List<OverdueItem> Items { get; set; }

        [JsonProperty("total")]
        public int Total { get; set; }
    }

    public class OverdueItem
    {
        [JsonProperty("id_task")]
        public int IdTask { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("deadline")]
        [JsonConverter(typeof(LocalDateConverter))]
        public DateTime? Deadline { get; set; }

        [JsonProperty("assignee")]
        public EmployeeBrief Assignee { get; set; }

        [JsonProperty("project")]
        public ReferenceItem Project { get; set; }
    }
}
