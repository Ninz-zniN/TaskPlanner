using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using TaskPlannerClient.Converters;

namespace TaskPlannerClient.Models
{
    public class TaskNote
    {
        [JsonProperty("id_note")]
        public int IdNote { get; set; }

        [JsonProperty("id_task")]
        public int IdTask { get; set; }

        [JsonProperty("content")]
        public string Content { get; set; }

        [JsonProperty("created_at")]
        [JsonConverter(typeof(LocalDateConverter))]
        public DateTime CreatedAt { get; set; }

        [JsonProperty("author")]
        public NoteAuthor Author { get; set; }
    }

    public class NoteAuthor
    {
        [JsonProperty("id_user")]
        public int IdUser { get; set; }

        [JsonProperty("login")]
        public string Login { get; set; }

        [JsonProperty("last_name")]
        public string LastName { get; set; }

        [JsonProperty("first_name")]
        public string FirstName { get; set; }
    }

    public class TaskNoteListResponse
    {
        [JsonProperty("items")]
        public List<TaskNote> Items { get; set; }

        [JsonProperty("total")]
        public int Total { get; set; }
    }
}
