using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace TaskPlannerClient.Converters
{
    public class LocalDateTimeConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(DateTime) || objectType == typeof(DateTime?);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.Value == null)
                return objectType == typeof(DateTime?) ? (DateTime?)null : DateTime.MinValue;

            if (reader.Value is DateTime dt)
            {
                if (dt.Kind == DateTimeKind.Utc)
                    return dt.ToLocalTime();
                return dt;
            }

            if (DateTime.TryParse(reader.Value.ToString(), out var parsed))
            {
                if (parsed.Kind == DateTimeKind.Utc)
                    return parsed.ToLocalTime();
                return parsed;
            }

            return objectType == typeof(DateTime?) ? (DateTime?)null : DateTime.MinValue;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value is DateTime dt)
                writer.WriteValue(dt.ToString("yyyy-MM-dd HH:mm:ss"));
            else
                writer.WriteNull();
        }
    }

    /// <summary>
    /// При сериализации выводит только дату (yyyy-MM-dd).
    /// При десериализации создаёт локальную дату, игнорируя смещение UTC.
    /// </summary>
    public class LocalDateConverter : JsonConverter<DateTime>
    {
        public override DateTime ReadJson(JsonReader reader, Type objectType, DateTime existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.Value == null) return DateTime.MinValue;
            if (reader.Value is DateTime dt)
            {
                // Если дата с Z (UTC) – переводим в локальное время
                if (dt.Kind == DateTimeKind.Utc)
                    return dt.ToLocalTime().Date;
                return dt.Date;
            }
            // Если пришла строка – парсим как локальную дату
            if (DateTime.TryParse(reader.Value.ToString(), out var parsed))
                return parsed.Date;
            return DateTime.MinValue;
        }

        public override void WriteJson(JsonWriter writer, DateTime value, JsonSerializer serializer)
        {
            writer.WriteValue(value.ToString("yyyy-MM-dd"));
        }
    }
}
