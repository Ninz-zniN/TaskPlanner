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
using TaskPlannerClient.Models;
using TaskPlannerClient.Service;

namespace TaskPlannerClient.Views
{
    /// <summary>
    /// Логика взаимодействия для ProjectDeviationBarWindow.xaml
    /// </summary>
    public partial class ProjectDeviationBarWindow : Window
    {
        public ObservableCollection<ISeries> Series { get; set; } = new();
        public ObservableCollection<Axis> XAxes { get; set; } = new();
        public ObservableCollection<Axis> YAxes { get; set; } = new();

        public ProjectDeviationBarWindow()
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
                // Показываем только завершённые проекты
                ProjectCombo.ItemsSource = projects.Where(p => p.Status == "completed").ToList();
                if (ProjectCombo.ItemsSource.Cast<Project>().Any())
                    ProjectCombo.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки проектов: {ex.Message}");
            }
        }

        private async void ProjectCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ProjectCombo.SelectedValue is int projectId)
            {
                try
                {
                    var data = await UserSession.Instance.Api.GetProjectSummaryReportAsync(projectId);

                    // Вычисляем отклонения и разделяем на положительные и отрицательные
                    var items = data.Tasks
                        .Select(t => new
                        {
                            t.Title,
                            Deviation = (t.ActualHours ?? 0) - t.EstimateHours
                        })
                        .OrderByDescending(t => Math.Abs(t.Deviation))
                        .ToList();

                    var names = items.Select(t => t.Title).ToArray();
                    var positiveValues = items.Select(t => t.Deviation > 0 ? (double)t.Deviation : 0).ToArray();
                    var negativeValues = items.Select(t => t.Deviation < 0 ? (double)t.Deviation : 0).ToArray();

                    Series.Clear();
                    XAxes.Clear();
                    YAxes.Clear();

                    // Серия перерасхода (красные столбцы)
                    Series.Add(new RowSeries<double>
                    {
                        Name = "Перерасход (ч)",
                        Values = positiveValues,
                        Fill = new SolidColorPaint(SKColor.Parse("#E57373")),
                        Stroke = new SolidColorPaint(SKColor.Parse("#C62828")) { StrokeThickness = 2 },
                        DataLabelsPaint = new SolidColorPaint(new SKColor(30, 30, 30)),
                        DataLabelsFormatter = point => point.Model > 0 ? $"+{point.Model:F1} ч" : "",
                        DataLabelsPosition = LiveChartsCore.Measure.DataLabelsPosition.Right,
                        Padding = 2
                    });

                    // Серия экономии (зелёные столбцы)
                    Series.Add(new RowSeries<double>
                    {
                        Name = "Экономия (ч)",
                        Values = negativeValues,
                        Fill = new SolidColorPaint(SKColor.Parse("#81C784")),
                        Stroke = new SolidColorPaint(SKColor.Parse("#2E7D32")) { StrokeThickness = 2 },
                        DataLabelsPaint = new SolidColorPaint(new SKColor(30, 30, 30)),
                        DataLabelsFormatter = point => point.Model < 0 ? $"{point.Model:F1} ч" : "",
                        DataLabelsPosition = LiveChartsCore.Measure.DataLabelsPosition.Right,
                        Padding = 2
                    });

                    XAxes.Add(new Axis
                    {
                        Name = "Часы",
                        NamePaint = new SolidColorPaint(new SKColor(66, 66, 66))
                    });

                    YAxes.Add(new Axis
                    {
                        Labels = names,
                        NamePaint = new SolidColorPaint(new SKColor(66, 66, 66))
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка загрузки данных: {ex.Message}");
                }
            }
        }
    }
}
