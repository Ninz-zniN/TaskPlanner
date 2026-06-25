using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using TaskPlannerClient.Converters;

namespace TaskPlannerClient.Models
{
    public class BurndownReport
    {
        [JsonProperty("sprint")]
        public BurndownSprintInfo Sprint { get; set; }

        [JsonProperty("ideal_burn")]
        public List<BurndownPoint> IdealBurn { get; set; }

        [JsonProperty("actual_burn")]
        public List<BurndownPoint> ActualBurn { get; set; }
    }

    public class BurndownSprintInfo
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("start_date")]
        [JsonConverter(typeof(LocalDateConverter))]
        public DateTime StartDate { get; set; }

        [JsonProperty("end_date")]
        [JsonConverter(typeof(LocalDateConverter))]
        public DateTime EndDate { get; set; }

        [JsonProperty("work_days")]
        public int WorkDays { get; set; }

        [JsonProperty("initial_hours")]
        public decimal InitialHours { get; set; }

        [JsonProperty("remaining_hours")]
        public decimal RemainingHours { get; set; }
    }

    public class BurndownPoint
    {
        [JsonProperty("day")]
        public int Day { get; set; }

        [JsonProperty("remaining")]
        public decimal Remaining { get; set; }
    }
}
