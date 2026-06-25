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
    /// Логика взаимодействия для ProjectSummaryReportControl.xaml
    /// </summary>
    public partial class ProjectSummaryReportControl : UserControl
    {
        public ObservableCollection<ProjectSummaryTask> Tasks { get; } = new();

        /// <summary>
        /// Возвращает сводную информацию о проекте для экспорта.
        /// </summary>
        public string ProjectSummaryText => SummaryText.Text;

        /// <summary>
        /// Возвращает текст с общим отклонением для экспорта.
        /// </summary>
        public string ProjectTotalText => TotalText.Text;

        public ProjectSummaryReportControl()
        {
            InitializeComponent();
            TasksGrid.ItemsSource = Tasks;
            Loaded += async (s, e) => await LoadProjects();
        }

        private async System.Threading.Tasks.Task LoadProjects()
        {
            try
            {
                var api = UserSession.Instance.Api;
                var projects = await api.GetProjectsAsync();
                var completedProjects = projects.Where(p => p.Status == "completed").ToList();
                ProjectCombo.ItemsSource = completedProjects;
                if (completedProjects.Any())
                    ProjectCombo.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки проектов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void ProjectCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ProjectCombo.SelectedValue is int projectId)
            {
                try
                {
                    var report = await UserSession.Instance.Api.GetProjectSummaryReportAsync(projectId);
                    SummaryText.Text = $"Проект: {report.Project.Name}\n" +
                       $"Описание: {report.Project.Description}\n" +
                       $"Завершён: {report.Project.CompletedAt:dd.MM.yyyy}\n" +
                       $"Задач в проекте: {report.TaskCount}\n" +
                       $"План: {report.PlannedHours} ч, Факт: {report.ActualHours} ч";

                    Tasks.Clear();
                    foreach (var t in report.Tasks)
                        Tasks.Add(t);
                    TotalText.Text = $"Общее отклонение: {report.ActualHours - report.PlannedHours:+#;-#;0} ч";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка загрузки сводки: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void DeviationChartButton_Click(object sender, RoutedEventArgs e)
        {
            var window = new ProjectDeviationBarWindow();
            window.Owner = Window.GetWindow(this);
            window.ShowDialog();
        }
    }
}
