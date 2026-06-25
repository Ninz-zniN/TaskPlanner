using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using TaskPlannerClient.Converters;

namespace TaskPlannerClient.Models
{
    public class ProjectSummaryReport
    {
        [JsonProperty("project")]
        public ProjectSummaryInfo Project { get; set; }

        [JsonProperty("planned_hours")]
        public decimal PlannedHours { get; set; }

        [JsonProperty("actual_hours")]
        public decimal ActualHours { get; set; }

        [JsonProperty("task_count")]
        public int TaskCount { get; set; }

        [JsonProperty("tasks")]
        public List<ProjectSummaryTask> Tasks { get; set; }
    }

    public class ProjectSummaryInfo
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("completed_at")]
        [JsonConverter(typeof(LocalDateConverter))]
        public DateTime? CompletedAt { get; set; }
    }

    public class ProjectSummaryTask
    {
        [JsonProperty("id_task")]
        public int IdTask { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("estimate_hours")]
        public decimal EstimateHours { get; set; }

        [JsonProperty("actual_hours")]
        public decimal? ActualHours { get; set; }

        [JsonProperty("status_id")]
        public int StatusId { get; set; }

        [JsonProperty("work_hours")]
        public decimal WorkHours { get; set; }

        [JsonProperty("review_hours")]
        public decimal ReviewHours { get; set; }

        [JsonProperty("test_hours")]
        public decimal TestHours { get; set; }
    }
}
