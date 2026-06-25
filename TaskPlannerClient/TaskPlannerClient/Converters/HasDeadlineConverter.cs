using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Data;
using TaskPlannerClient.Models;

namespace TaskPlannerClient.Converters
{
    public class HasDeadlineConverter: IValueConverter
    {
        public Dictionary<DateTime, List<TaskItem>> TasksByDate { get; set; } = new();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime date && TasksByDate != null)
            {
                return TasksByDate.ContainsKey(date);
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
