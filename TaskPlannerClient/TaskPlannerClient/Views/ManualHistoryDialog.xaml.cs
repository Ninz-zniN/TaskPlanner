using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace TaskPlannerClient.Views
{
    /// <summary>
    /// Логика взаимодействия для ManualHistoryDialog.xaml
    /// </summary>
    public partial class ManualHistoryDialog : Window
    {
        public DateTime? SelectedDate { get; private set; }
        public decimal? ActualHours { get; private set; }

        public ManualHistoryDialog()
        {
            InitializeComponent();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ChangeDatePicker.SelectedDate.HasValue)
            {
                MessageBox.Show("Укажите дату.");
                return;
            }

            SelectedDate = ChangeDatePicker.SelectedDate.Value;

            if (decimal.TryParse(HoursTextBox.Text, out var hours) && hours >= 0)
                ActualHours = hours;
            else
                ActualHours = 0;

            this.DialogResult = true;
            this.Close();
        }
    }
}
