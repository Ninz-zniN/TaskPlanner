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
    /// Логика взаимодействия для StatusHistoryWindow.xaml
    /// </summary>
    public partial class StatusHistoryWindow : Window
    {
        public ObservableCollection<ISeries> Series { get; set; } = new();
        public ObservableCollection<Axis> XAxes { get; set; } = new();
        public ObservableCollection<Axis> YAxes { get; set; } = new();

        public StatusHistoryWindow()
        {
            InitializeComponent();
            DataContext = this;
            Loaded += async (s, e) => await LoadSprints();
        }

        private async System.Threading.Tasks.Task LoadSprints()
        {
            try
            {
                var api = UserSession.Instance.Api;
                var sprints = await api.GetSprintsAsync();
                SprintCombo.ItemsSource = sprints;
                if (sprints.Any())
                    SprintCombo.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки спринтов: {ex.Message}");
            }
        }

        private async void SprintCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SprintCombo.SelectedValue is int sprintId)
            {
                try
                {
                    var data = await UserSession.Instance.Api.GetStatusHistoryAsync(sprintId);
                    // Группируем данные по статусам и дням
                    var statuses = data.Days.Select(d => d.Status).Distinct().ToList();
                    // Исходные даты (полные) для сопоставления
                    var originalDates = data.Days.Select(d => d.Date).Distinct().OrderBy(d => d).ToList();
                    // Метки для оси X – только день
                    var dateLabels = originalDates.Select(d => DateTime.Parse(d).Day.ToString()).ToList();

                    Series.Clear();
                    XAxes.Clear();
                    YAxes.Clear();

                    var colors = new[] { "#90CAF9", "#FFCC80", "#CE93D8", "#A5D6A7", "#EF9A9A", "#B0BEC5" };
                    int colorIdx = 0;
                    foreach (var status in statuses)
                    {
                        var values = new double[originalDates.Count];
                        for (int i = 0; i < originalDates.Count; i++)
                        {
                            var item = data.Days.FirstOrDefault(d => d.Date == originalDates[i] && d.Status == status);
                            values[i] = item?.Count ?? 0;
                        }
                        Series.Add(new StackedAreaSeries<double>
                        {
                            Name = status,
                            Values = values,
                            Fill = new SolidColorPaint(SKColor.Parse(colors[colorIdx % colors.Length]).WithAlpha(128)),
                            Stroke = new SolidColorPaint(SKColor.Parse(colors[colorIdx % colors.Length])) { StrokeThickness = 2 },
                            GeometrySize = 0,
                            LineSmoothness = 0.3
                        });
                        colorIdx++;
                    }

                    XAxes.Add(new Axis
                    {
                        Labels = dateLabels,   // ← только дни
                        LabelsPaint = new SolidColorPaint(new SKColor(66, 66, 66))
                    });
                    YAxes.Add(new Axis
                    {
                        Name = "Количество задач",
                        NamePaint = new SolidColorPaint(new SKColor(66, 66, 66)),
                        MinLimit = 0
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка загрузки истории статусов: {ex.Message}");
                }
            }
        }
    }
}
