using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TaskPlannerClient.Models;
using TaskPlannerClient.Service;

namespace TaskPlannerClient.Views.Tabs.Manager
{
    /// <summary>
    /// Логика взаимодействия для OverdueReportControl.xaml
    /// </summary>
    public partial class OverdueReportControl : UserControl
    {
        public ObservableCollection<OverdueItem> OverdueItems { get; set; } = new ObservableCollection<OverdueItem>();

        public OverdueReportControl()
        {
            InitializeComponent();
            OverdueDataGrid.ItemsSource = OverdueItems;
            Loaded += async (s, e) => await LoadData();
        }

        private async System.Threading.Tasks.Task LoadData()
        {
            try
            {
                var report = await UserSession.Instance.Api.GetOverdueReportAsync();
                OverdueItems.Clear();
                foreach (var item in report.Items)
                    OverdueItems.Add(item);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки просроченных задач: {ex.Message}");
            }
        }

        private void OverdueByAssigneeButton_Click(object sender, RoutedEventArgs e)
        {
            var window = new OverdueByAssigneeBarWindow();
            window.Owner = Window.GetWindow(this);
            window.ShowDialog();
        }
    }
}
