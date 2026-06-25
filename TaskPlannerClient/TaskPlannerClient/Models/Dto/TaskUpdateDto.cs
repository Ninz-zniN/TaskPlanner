using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using static TaskPlannerClient.Converters.LocalDateTimeConverter;

namespace TaskPlannerClient.Models.Dto
{
    public class TaskUpdateDto
    {
        [JsonProperty("title")]
        public string? Title { get; set; }

        [JsonProperty("description")]
        public string? Description { get; set; }

        [JsonProperty("id_task_type")]
        public int? IdTaskType { get; set; }

        [JsonProperty("id_task_status")]
        public int? IdTaskStatus { get; set; }

        [JsonProperty("id_priority")]
        public int? IdPriority { get; set; }

        [JsonProperty("estimate_hours")]
        public decimal? EstimateHours { get; set; }

        [JsonProperty("deadline")]
        //[JsonConverter(typeof(LocalDateConverter))]
        public string? Deadline { get; set; }

        [JsonProperty("id_assignee")]
        public int? IdAssignee { get; set; }

        [JsonProperty("id_sprint")]
        public int? IdSprint { get; set; }
    }
}
