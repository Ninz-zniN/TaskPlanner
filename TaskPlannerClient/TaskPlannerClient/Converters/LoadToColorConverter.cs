using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Data;
using System.Windows.Media;

namespace TaskPlannerClient.Converters
{
    public class LoadToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is decimal load)
            {
                if (load > 100) return new SolidColorBrush(Colors.IndianRed);   // перегрузка
                if (load > 80) return new SolidColorBrush(Colors.LightGreen);   // норма
                return new SolidColorBrush(Colors.Khaki);                      // недогрузка
            }
            return new SolidColorBrush(Colors.Transparent);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
