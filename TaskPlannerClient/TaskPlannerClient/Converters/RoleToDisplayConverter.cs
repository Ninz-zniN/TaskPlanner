using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace TaskPlannerClient.Converters
{
    public class RoleToDisplayConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value?.ToString() switch
            {
                "worker" => "Работник",
                "project_manager" => "Менеджер",
                "admin" => "Администратор",
                _ => value?.ToString() ?? ""
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
