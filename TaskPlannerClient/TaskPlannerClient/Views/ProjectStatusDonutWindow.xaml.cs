using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
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
using System.Windows.Shapes;
using TaskPlannerClient.Service;

namespace TaskPlannerClient.Views
{
    /// <summary>
    /// Логика взаимодействия для ProjectStatusDonutWindow.xaml
    /// </summary>
    public partial class ProjectStatusDonutWindow : Window
    {
        public ObservableCollection<ISeries> Series { get; set; } = new();

        public ProjectStatusDonutWindow()
        {
            InitializeComponent();
            DataContext = this;
            Loaded += async (s, e) => await LoadProjects();
        }

        private async System.Threading.Tasks.Task LoadProjects()
        {
            try
            {
                var api = UserSession.Instance.Api;
                var projects = await api.GetProjectsAsync();
                ProjectCombo.ItemsSource = projects;
                if (projects.Any())
                    ProjectCombo.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки проектов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void ProjectCombo_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (ProjectCombo.SelectedValue is int projectId)
            {
                try
                {
                    var data = await UserSession.Instance.Api.GetProjectStatusDistributionAsync(projectId);
                    var colors = new[] { "#90CAF9", "#CE93D8", "#FFCC80", "#A5D6A7", "#EF9A9A", "#B0BEC5" };

                    Series.Clear();
                    int i = 0;
                    foreach (var item in data.Items)
                    {
                        Series.Add(new PieSeries<int>
                        {
                            Name = item.Status,
                            Values = new[] { item.Count },
                            Fill = new SolidColorPaint(SKColor.Parse(colors[i % colors.Length])),
                            DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Outer,
                            DataLabelsPaint = new SolidColorPaint(new SKColor(30, 30, 30)),
                            DataLabelsFormatter = point => $"{point.Model}"
                        });
                        i++;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка загрузки распределения: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
