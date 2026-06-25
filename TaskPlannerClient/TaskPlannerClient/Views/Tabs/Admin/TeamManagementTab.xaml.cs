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

namespace TaskPlannerClient.Views.Tabs.Admin
{
    /// <summary>
    /// Логика взаимодействия для TeamManagementTab.xaml
    /// </summary>
    public partial class TeamManagementTab : UserControl
    {
        public ObservableCollection<Team> Teams { get; set; } = new ObservableCollection<Team>();

        public TeamManagementTab()
        {
            InitializeComponent();
            TeamsGrid.ItemsSource = Teams;
            Loaded += async (s, e) => await LoadTeams();
        }

        private async System.Threading.Tasks.Task LoadTeams()
        {
            try
            {
                var teams = await DataCache.GetTeamsAsync(true); //await UserSession.Instance.Api.GetTeamsAsync();
                Teams.Clear();
                foreach (var team in teams)
                    Teams.Add(team);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки команд: {ex.Message}");
            }
        }

        private async void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var window = new TeamEditWindow();
            window.Owner = Window.GetWindow(this);
            if (window.ShowDialog() == true)
                await LoadTeams();
        }

        private async void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is Team team)
            {
                var window = new TeamEditWindow(team);
                window.Owner = Window.GetWindow(this);
                if (window.ShowDialog() == true)
                    await LoadTeams();
            }
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is Team team)
            {
                var result = MessageBox.Show($"Удалить команду '{team.TeamName}'?", "Подтверждение",
                    MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        await UserSession.Instance.Api.DeleteTeamAsync(team.IdTeam);
                        await LoadTeams();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка удаления: {ex.Message}");
                    }
                }
            }
        }

        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            await LoadTeams();
        }
    }
}
