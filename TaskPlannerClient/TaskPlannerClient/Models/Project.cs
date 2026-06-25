using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace TaskPlannerClient.Models
{
    public class Project
    {
        [JsonProperty("id_project")]
        public int IdProject { get; set; }

        [JsonProperty("project_name")]
        public string ProjectName { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("manager")]
        public EmployeeBrief Manager { get; set; }

        [JsonProperty("team")]
        public Team Team { get; set; }
        /// <summary>
        /// Статус проекта на русском языке для отображения в UI.
        /// Игнорируется при сериализации, чтобы не отправлять на сервер.
        /// </summary>
        [JsonIgnore]
        public string StatusDisplay => Status switch
        {
            "planning" => "Планирование",
            "active" => "Активный",
            "completed" => "Завершён",
            _ => Status ?? ""
        };
    }

    public class ProjectListResponse
    {
        [JsonProperty("items")]
        public List<Project> Items { get; set; }

        [JsonProperty("total")]
        public int Total { get; set; }
    }
}
