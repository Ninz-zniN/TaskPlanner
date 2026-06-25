using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace TaskPlannerClient.Models.Dto
{
    public class CreateProjectDto
    {
        [JsonProperty("project_name")]
        public string ProjectName { get; }

        [JsonProperty("description")]
        public string? Description { get; }

        [JsonProperty("id_manager")]
        public int? IdManager { get; }

        [JsonProperty("id_team")]
        public int? IdTeam { get; }

        [JsonProperty("status")]
        public string? Status { get; }  // по умолчанию будет "planning" на сервере, но можно передать

        public CreateProjectDto(string projectName, string? description = null, int? idManager = null, int? idTeam = null, string? status = null)
        {
            ProjectName = projectName;
            Description = description;
            IdManager = idManager;
            IdTeam = idTeam;
            Status = status;
        }
    }
}
