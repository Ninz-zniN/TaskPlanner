using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace TaskPlannerClient.Models
{
    public class ProjectProgressItem
    {
        [JsonProperty("id_project")]
        public int IdProject { get; set; }

        [JsonProperty("project_name")]
        public string ProjectName { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("planned_hours")]
        public decimal PlannedHours { get; set; }

        [JsonProperty("actual_hours")]
        public decimal ActualHours { get; set; }

        [JsonProperty("total_tasks")]
        public int TotalTasks { get; set; }

        [JsonProperty("completed_tasks")]
        public int CompletedTasks { get; set; }

        [JsonProperty("completion_percent_tasks")]
        public decimal CompletionPercentTasks { get; set; }

        [JsonProperty("completion_percent_hours")]
        public decimal CompletionPercentHours { get; set; }

        [JsonProperty("completion_percent_weighted")]
        public decimal CompletionPercentWeighted { get; set; }
    }

    public class ProjectProgressReport
    {
        [JsonProperty("items")]
        public List<ProjectProgressItem> Items { get; set; }

        [JsonProperty("total")]
        public int Total { get; set; }
    }
}
