using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace TaskPlannerClient.Models
{
    public class OverdueByAssigneeItem
    {
        [JsonProperty("employee")]
        public EmployeeBrief Employee { get; set; }

        [JsonProperty("overdue_count")]
        public int OverdueCount { get; set; }
    }

    public class OverdueByAssigneeReport
    {
        [JsonProperty("items")]
        public List<OverdueByAssigneeItem> Items { get; set; }

        [JsonProperty("total")]
        public int Total { get; set; }
    }
}
