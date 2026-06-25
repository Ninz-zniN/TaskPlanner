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
    /// Логика взаимодействия для LoadReportControl.xaml
    /// </summary>
    public partial class LoadReportControl : UserControl
    {
        private LoadReport _lastReport;
        public ObservableCollection<LoadItem> LoadItems { get; set; } = new ObservableCollection<LoadItem>();

        /// <summary>
        /// Возвращает название выбранной команды для отображения в экспорте.
        /// </summary>
        public string SelectedTeamName
        {
            get
            {
                if (TeamFilterCombo.SelectedItem is Team team)
                    return team.TeamName ?? "Все";
                return "Все";
            }
        }

        public LoadReportControl()
        {
            InitializeComponent();
            LoadDataGrid.ItemsSource = LoadItems;
            Loaded += OnLoaded;
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var api = UserSession.Instance.Api;
                // Загружаем команды для фильтра
                var teams = await DataCache.GetTeamsAsync(true);//api.GetTeamsAsync();
                var teamItems = new List<Team>
                {
                    new Team { IdTeam = -1, TeamName = "Все" }
                };
                teamItems.AddRange(teams);
                TeamFilterCombo.ItemsSource = teamItems;
                TeamFilterCombo.SelectedIndex = 0;

                await LoadDataAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки: {ex.Message}");
            }
        }

        private async System.Threading.Tasks.Task LoadDataAsync(int? teamId = null)
        {
            try
            {
                var api = UserSession.Instance.Api;
                _lastReport = await api.GetLoadReportAsync(teamId: teamId);
                LoadItems.Clear();
                foreach (var item in _lastReport.Items)
                    LoadItems.Add(item);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки отчёта: {ex.Message}");
            }
        }

        private async void TeamFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int? teamId = (TeamFilterCombo.SelectedValue as int?) == -1 ? null : TeamFilterCombo.SelectedValue as int?;
            await LoadDataAsync(teamId);
        }

        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            int? teamId = (TeamFilterCombo.SelectedValue as int?) == -1 ? null : TeamFilterCombo.SelectedValue as int?;
            await LoadDataAsync(teamId);
        }

        private void TopEmployeesButton_Click(object sender, RoutedEventArgs e)
        {
            var window = new TopEmployeesBarWindow();
            window.Owner = Window.GetWindow(this);
            window.ShowDialog();
        }
    }
}
