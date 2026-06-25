using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using static TaskPlannerClient.Converters.LocalDateTimeConverter;

namespace TaskPlannerClient.Models.Dto
{
    public class CreateTaskDto
    {
        [JsonProperty("title")]
        public string Title { get; }

        [JsonProperty("description")]
        public string? Description { get; }

        [JsonProperty("id_task_type")]
        public int? IdTaskType { get; }

        [JsonProperty("id_priority")]
        public int? IdPriority { get; }

        [JsonProperty("estimate_hours")]
        public decimal EstimateHours { get; }

        [JsonProperty("deadline")]
        //[JsonConverter(typeof(LocalDateConverter))]
        public string? Deadline { get; } // yyyy-MM-dd

        [JsonProperty("id_assignee")]
        public int? IdAssignee { get; }

        [JsonProperty("id_project")]
        public int? IdProject { get; }

        [JsonProperty("id_sprint")]
        public int? IdSprint { get; }

        [JsonProperty("id_task_status")]
        public int? IdTaskStatus { get; }

        public CreateTaskDto(
            string title,
            string? description = null,
            int? idTaskType = null,
            int? idPriority = null,
            decimal estimateHours = 0,
            string? deadline = null,
            int? idAssignee = null,
            int? idProject = null,
            int? idSprint = null,
            int? idTaskStatus = null)
        {
            Title = title;
            Description = description;
            IdTaskType = idTaskType;
            IdPriority = idPriority;
            EstimateHours = estimateHours;
            Deadline = deadline;
            IdAssignee = idAssignee;
            IdProject = idProject;
            IdSprint = idSprint;
            IdTaskStatus = idTaskStatus;
        }
    }
}
