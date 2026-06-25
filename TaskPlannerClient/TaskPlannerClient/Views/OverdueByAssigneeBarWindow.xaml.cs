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
    /// Логика взаимодействия для OverdueByAssigneeBarWindow.xaml
    /// </summary>
    public partial class OverdueByAssigneeBarWindow : Window
    {
        public ObservableCollection<ISeries> Series { get; set; } = new();
        public ObservableCollection<Axis> XAxes { get; set; } = new();
        public ObservableCollection<Axis> YAxes { get; set; } = new();

        public OverdueByAssigneeBarWindow()
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
                int? teamId = (TeamFilterCombo.SelectedValue as int?) == -1 ? null : TeamFilterCombo.SelectedValue as int?;

                var api = UserSession.Instance.Api;
                var data = await api.GetOverdueByAssigneeAsync(teamId);

                var names = data.Items.Select(i => $"{i.Employee.LastName} {i.Employee.FirstName}").ToArray();
                var counts = data.Items.Select(i => (double)i.OverdueCount).ToArray();

                Series.Clear();
                XAxes.Clear();
                YAxes.Clear();

                Series.Add(new RowSeries<double>
                {
                    Name = "Просрочено задач",
                    Values = counts,
                    Fill = new SolidColorPaint(SKColor.Parse("#EF9A9A")),
                    Stroke = new SolidColorPaint(SKColor.Parse("#C62828")) { StrokeThickness = 2 },
                    DataLabelsPaint = new SolidColorPaint(new SKColor(30, 30, 30)),
                    DataLabelsFormatter = point => $"{point.Model}",
                    DataLabelsPosition = LiveChartsCore.Measure.DataLabelsPosition.Right,
                    Padding = 5
                });

                XAxes.Add(new Axis
                {
                    Name = "Количество задач",
                    NamePaint = new SolidColorPaint(new SKColor(66, 66, 66)),
                    MinLimit = 0,
                    MinStep = 1,
                    ForceStepToMin = true
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

        private async void TeamFilter_SelectionChanged(object sender, SelectionChangedEventArgs e) => await LoadData();
        private async void RefreshButton_Click(object sender, RoutedEventArgs e) => await LoadData();
    }
}
