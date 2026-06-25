using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace TaskPlannerClient.Models.Dto
{
    public class ProjectUpdateDto
    {
        [JsonProperty("project_name")]
        public string? ProjectName { get; set; }

        [JsonProperty("description")]
        public string? Description { get; set; }

        [JsonProperty("id_manager")]
        public int? IdManager { get; set; }

        [JsonProperty("id_team")]
        public int? IdTeam { get; set; }

        [JsonProperty("status")]
        public string? Status { get; set; }
    }
}
