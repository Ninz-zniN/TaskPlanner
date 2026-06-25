using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace TaskPlannerClient.Converters
{
    public class ProjectStatusConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value?.ToString() switch
            {
                "planning" => "Планирование",
                "active" => "Активный",
                "completed" => "Завершён",
                _ => value?.ToString() ?? ""
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
