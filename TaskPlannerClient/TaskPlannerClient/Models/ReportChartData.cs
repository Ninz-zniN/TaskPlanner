using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace TaskPlannerClient.Models
{
    public class StatusDistributionItem
    {
        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("count")]
        public int Count { get; set; }
    }

    public class StatusDistributionResponse
    {
        [JsonProperty("items")]
        public List<StatusDistributionItem> Items { get; set; }
    }

    public class TopEmployeeItem
    {
        [JsonProperty("employee")]
        public EmployeeBrief Employee { get; set; }

        [JsonProperty("total_hours")]
        public decimal TotalHours { get; set; }

        [JsonProperty("capacity_hours")]
        public decimal CapacityHours { get; set; }

        [JsonProperty("load_percent")]
        public decimal LoadPercent { get; set; }
    }

    public class TopEmployeesResponse
    {
        [JsonProperty("items")]
        public List<TopEmployeeItem> Items { get; set; }
    }
}
