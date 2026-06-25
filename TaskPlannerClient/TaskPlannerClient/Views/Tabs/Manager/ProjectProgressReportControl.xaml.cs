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
    /// Логика взаимодействия для ProjectProgressReportControl.xaml
    /// </summary>
    public partial class ProjectProgressReportControl : UserControl
    {
        public ObservableCollection<ProjectProgressItem> Items { get; } = new();

        public ProjectProgressReportControl()
        {
            InitializeComponent();
            ProgressGrid.ItemsSource = Items;
            Loaded += async (s, e) => await LoadData();
        }

        private async System.Threading.Tasks.Task LoadData()
        {
            try
            {
                var report = await UserSession.Instance.Api.GetProjectProgressReportAsync();
                Items.Clear();
                foreach (var item in report.Items)
                    Items.Add(item);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки прогресса проектов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OpenDonutButton_Click(object sender, RoutedEventArgs e)
        {
            var window = new ProjectStatusDonutWindow();
            window.Owner = Window.GetWindow(this);  // родительское окно — ReportWindow
            window.ShowDialog();
        }
    }
}
