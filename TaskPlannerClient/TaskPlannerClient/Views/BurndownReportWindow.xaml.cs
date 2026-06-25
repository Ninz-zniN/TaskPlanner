using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.Measure;
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
    /// Логика взаимодействия для BurndownReportWindow.xaml
    /// </summary>
    public partial class BurndownReportWindow : Window
    {
        public ObservableCollection<ISeries> Series { get; set; } = new();
        public ObservableCollection<ICartesianAxis> XAxes { get; set; } = new();
        public ObservableCollection<ICartesianAxis> YAxes { get; set; } = new();

        public BurndownReportWindow()
        {
            InitializeComponent();
            DataContext = this;
            Loaded += async (s, e) => await LoadSprints();
        }

        private async System.Threading.Tasks.Task LoadSprints()
        {
            var api = UserSession.Instance.Api;
            var sprints = await api.GetSprintsAsync();
            SprintCombo.ItemsSource = sprints;
            // По умолчанию выбираем активный спринт
            var active = sprints.FirstOrDefault(s => s.IsActive) ?? sprints.FirstOrDefault();
            if (active != null)
                SprintCombo.SelectedValue = active.IdSprint;
        }

        private async void SprintCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SprintCombo.SelectedValue is int sprintId)
                await LoadData(sprintId);
        }

        private async System.Threading.Tasks.Task LoadData(int sprintId)
        {
            try
            {
                var report = await UserSession.Instance.Api.GetBurndownReportAsync(sprintId);
                SprintTitle.Text =
                    $"Спринт: {report.Sprint.Name} | Начальные часы: {report.Sprint.InitialHours} | " +
                    $"Осталось: {report.Sprint.RemainingHours} | Дней: {report.Sprint.WorkDays}";

                var idealColor = new SolidColorPaint(new SKColor(103, 58, 183)) { StrokeThickness = 2 };
                var actualColor = new SolidColorPaint(new SKColor(198, 40, 40)) { StrokeThickness = 3 };

                var idealSeries = new LineSeries<ObservablePoint>
                {
                    Name = "Идеальный остаток",
                    Values = report.IdealBurn
                        .Select(p => new ObservablePoint(p.Day, (double)p.Remaining))
                        .ToList(),
                    Stroke = idealColor,
                    GeometryStroke = idealColor,
                    GeometryFill = idealColor,
                    Fill = null,
                    GeometrySize = 10,
                    LineSmoothness = 0,
                    YToolTipLabelFormatter = point => $"{point.Coordinate.PrimaryValue:F1} ч"
                };

                var actualSeries = new LineSeries<ObservablePoint>
                {
                    Name = "Фактический остаток",
                    Values = report.ActualBurn?.Any() == true
                        ? report.ActualBurn
                            .Select(p => new ObservablePoint(p.Day, (double)p.Remaining))
                            .ToList()
                        : new List<ObservablePoint>(),
                    Stroke = actualColor,
                    GeometryStroke = actualColor,
                    GeometryFill = actualColor,
                    Fill = null,
                    GeometrySize = 10,
                    LineSmoothness = 0,
                    YToolTipLabelFormatter = point => $"{point.Coordinate.PrimaryValue:F1} ч"
                };

                Series.Clear();
                Series.Add(idealSeries);
                Series.Add(actualSeries);

                XAxes.Clear();
                XAxes.Add(new Axis
                {
                    Name = "День",
                    // Добавляем запас по 0.5 дня с каждой стороны, чтобы точки не обрезались
                    MinLimit = 1 - 0.5,
                    MaxLimit = report.Sprint.WorkDays + 0.5,
                    ForceStepToMin = true,
                    MinStep = 1,
                    NamePaint = new SolidColorPaint(new SKColor(66, 66, 66))
                });

                YAxes.Clear();
                YAxes.Add(new Axis
                {
                    Name = "Остаток часов",
                    // Небольшой отрицательный минимум, чтобы нижняя точка не прилипала
                    MinLimit = -5,
                    NamePaint = new SolidColorPaint(new SKColor(66, 66, 66))
                });
            }
            catch (Exception ex)
            {
                SprintTitle.Text = $"Ошибка загрузки: {ex.Message}";
            }
        }
    }
}
