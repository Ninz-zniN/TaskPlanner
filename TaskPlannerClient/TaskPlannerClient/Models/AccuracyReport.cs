using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace TaskPlannerClient.Models
{
    public class AccuracyReport
    {
        [JsonProperty("items")]
        public List<AccuracyItem> Items { get; set; }

        [JsonProperty("total_estimate")]
        public decimal TotalEstimate { get; set; }

        [JsonProperty("total_actual")]
        public decimal TotalActual { get; set; }

        [JsonProperty("overall_deviation_percent")]
        public decimal OverallDeviationPercent { get; set; }
    }

    public class AccuracyItem
    {
        [JsonProperty("task")]
        public AccuracyTaskInfo Task { get; set; }

        [JsonProperty("estimate_hours")]
        public decimal EstimateHours { get; set; }

        [JsonProperty("actual_hours")]
        public decimal ActualHours { get; set; }

        [JsonProperty("deviation")]
        public decimal Deviation { get; set; }

        [JsonProperty("deviation_percent")]
        public decimal DeviationPercent { get; set; }
    }

    public class AccuracyTaskInfo
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }
    }
}
