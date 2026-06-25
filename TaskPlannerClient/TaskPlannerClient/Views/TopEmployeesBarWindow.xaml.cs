using LiveChartsCore;
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
using TaskPlannerClient.Models;
using TaskPlannerClient.Service;

namespace TaskPlannerClient.Views
{
    /// <summary>
    /// Логика взаимодействия для TopEmployeesBarWindow.xaml
    /// </summary>
    public partial class TopEmployeesBarWindow : Window
    {
        public ObservableCollection<ISeries> Series { get; set; } = new();
        public ObservableCollection<Axis> XAxes { get; set; } = new();
        public ObservableCollection<Axis> YAxes { get; set; } = new();

        public TopEmployeesBarWindow()
        {
            InitializeComponent();
            DataContext = this;
            Loaded += async (s, e) => await LoadFilters();
        }

        private async System.Threading.Tasks.Task LoadFilters()
        {
            try
            {
                var api = UserSession.Instance.Api;
                var teams = await api.GetTeamsAsync();
                var teamItems = new List<Team>
                {
                    new Team { IdTeam = -1, TeamName = "Все" }
                };
                teamItems.AddRange(teams);
                TeamFilterCombo.ItemsSource = teamItems;
                TeamFilterCombo.SelectedIndex = 0;
                await LoadData();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки фильтров: {ex.Message}");
            }
        }

        private async System.Threading.Tasks.Task LoadData()
        {
            try
            {
                int limit = int.TryParse(LimitTextBox.Text, out var l) ? l : 5;
                int? teamId = (TeamFilterCombo.SelectedValue as int?) == -1 ? null : TeamFilterCombo.SelectedValue as int?;

                var api = UserSession.Instance.Api;
                // Передаём team_id на сервер
                var data = await api.GetTopEmployeesAsync(limit, teamId);

                // Подписи по оси Y – фамилия и имя
                var names = data.Items.Select(i => $"{i.Employee.LastName} {i.Employee.FirstName}").ToArray();
                var factHours = data.Items.Select(i => (double)i.TotalHours).ToArray();
                var capHours = data.Items.Select(i => (double)i.CapacityHours).ToArray();

                Series.Clear();
                XAxes.Clear();
                YAxes.Clear();

                // Серия факта
                var factSeries = new RowSeries<double>
                {
                    Name = "Факт",
                    Values = factHours,
                    Fill = new SolidColorPaint(SKColor.Parse("#CE93D8")),
                    Stroke = new SolidColorPaint(SKColor.Parse("#9C27B0")) { StrokeThickness = 2 },
                    // Отступы между группами
                    Padding = 5
                };
                factSeries.DataLabelsPaint = new SolidColorPaint(new SKColor(30, 30, 30));
                factSeries.DataLabelsFormatter = point => $"{point.Model:F1} ч";
                factSeries.DataLabelsPosition = LiveChartsCore.Measure.DataLabelsPosition.Right;
                Series.Add(factSeries);

                // Серия нормы
                var capSeries = new RowSeries<double>
                {
                    Name = "Норма",
                    Values = capHours,
                    Fill = new SolidColorPaint(SKColor.Parse("#90CAF9")),
                    Stroke = new SolidColorPaint(SKColor.Parse("#1976D2")) { StrokeThickness = 2 },
                    Padding = 5
                };
                capSeries.DataLabelsPaint = new SolidColorPaint(new SKColor(30, 30, 30));
                capSeries.DataLabelsFormatter = point => $"{point.Model:F1} ч";
                capSeries.DataLabelsPosition = LiveChartsCore.Measure.DataLabelsPosition.Right;
                Series.Add(capSeries);

                XAxes.Add(new Axis
                {
                    Name = "Часы",
                    NamePaint = new SolidColorPaint(new SKColor(66, 66, 66)),
                    MinLimit = 0,
                    MinStep = 15,          // минимальный шаг 15 часов
                    ForceStepToMin = true  // принудительно использовать этот шаг
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

        private void RefreshData(object sender, RoutedEventArgs e) => _ = LoadData();
        private async void RefreshButton_Click(object sender, RoutedEventArgs e) => await LoadData();
    }
}
