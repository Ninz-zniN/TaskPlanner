using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace TaskPlannerClient.Models.Dto
{
    public class TeamUpdateDto
    {
        [JsonProperty("team_name")]
        public string? TeamName { get; set; }

        [JsonProperty("team_lead_id")]
        public int? TeamLeadId { get; set; }
    }
}
