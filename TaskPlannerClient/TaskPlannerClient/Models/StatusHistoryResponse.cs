using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace TaskPlannerClient.Models
{
    public class StatusHistoryItem
    {
        [JsonProperty("date")]
        public string Date { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("count")]
        public int Count { get; set; }
    }

    public class StatusHistoryResponse
    {
        [JsonProperty("sprint")]
        public string SprintName { get; set; }

        [JsonProperty("days")]
        public List<StatusHistoryItem> Days { get; set; }
    }
}
