using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace TaskPlannerClient.Models
{
    public class LoadReport
    {
        [JsonProperty("sprint")]
        public SprintInfo Sprint { get; set; }

        [JsonProperty("items")]
        public List<LoadItem> Items { get; set; }
    }

    public class SprintInfo
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("work_days")]
        public int WorkDays { get; set; }
    }

    public class LoadItem
    {
        [JsonProperty("employee")]
        public EmployeeBrief Employee { get; set; }

        [JsonProperty("current_load_hours")]
        public decimal CurrentLoadHours { get; set; }

        [JsonProperty("capacity_hours")]
        public decimal CapacityHours { get; set; }

        [JsonProperty("load_percent")]
        public decimal LoadPercent { get; set; }
    }
}
