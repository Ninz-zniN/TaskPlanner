using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using TaskPlannerClient.Converters;

namespace TaskPlannerClient.Models
{
    public class TaskItem
    {
        [JsonProperty("id_task")]
        public int IdTask { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("task_type")]
        public ReferenceItem TaskType { get; set; }

        [JsonProperty("status")]
        public ReferenceItem Status { get; set; }

        [JsonProperty("priority")]
        public PriorityItem Priority { get; set; }

        [JsonProperty("estimate_hours")]
        public decimal EstimateHours { get; set; }

        [JsonProperty("actual_hours")]
        public decimal? ActualHours { get; set; }

        [JsonProperty("deadline")]
        [JsonConverter(typeof(LocalDateConverter))]
        public DateTime? Deadline { get; set; }

        [JsonProperty("assignee")]
        public EmployeeBrief Assignee { get; set; }

        [JsonProperty("project")]
        public ReferenceItem Project { get; set; }

        [JsonProperty("sprint")]
        public ReferenceItem Sprint { get; set; }

        [JsonProperty("created_at")]
        [JsonConverter(typeof(LocalDateTimeConverter))]
        public DateTime CreatedAt { get; set; }

        [JsonProperty("updated_at")]
        [JsonConverter(typeof(LocalDateTimeConverter))]
        public DateTime? UpdatedAt { get; set; }
        
        [JsonIgnore]
        public bool IsAwaiting { get; set; } = false;
    }

    public class EmployeeBrief
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("last_name")]
        public string LastName { get; set; }
        
        [JsonProperty("first_name")]
        public string FirstName { get; set; }
    }

    public class ReferenceItem
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public class PriorityItem : ReferenceItem
    {
        [JsonProperty("weight")]
        public int Weight { get; set; }
    }

    public class TaskListResponse
    {
        [JsonProperty("items")]
        public List<TaskItem> Items { get; set; }

        [JsonProperty("total")]
        public int Total { get; set; }
    }
}
