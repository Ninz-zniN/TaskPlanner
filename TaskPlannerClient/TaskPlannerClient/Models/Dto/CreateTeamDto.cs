using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace TaskPlannerClient.Models.Dto
{
    public class CreateTeamDto
    {
        [JsonProperty("team_name")]
        public string TeamName { get; }

        [JsonProperty("team_lead_id")]
        public int? TeamLeadId { get; }

        public CreateTeamDto(string teamName, int? teamLeadId = null)
        {
            TeamName = teamName;
            TeamLeadId = teamLeadId;
        }
    }
}
