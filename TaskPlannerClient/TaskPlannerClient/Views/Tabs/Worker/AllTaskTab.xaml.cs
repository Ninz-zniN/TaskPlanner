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

namespace TaskPlannerClient.Views.Tabs.Worker
{
    /// <summary>
    /// Логика взаимодействия для AllTaskTab.xaml
    /// </summary>
    public partial class AllTaskTab : UserControl
    {
        private List<Employee> _teamMembers;
        private ObservableCollection<TaskItem> _tasks;
        private string _searchText = "";

        public AllTaskTab()
        {
            InitializeComponent();
            _tasks = new ObservableCollection<TaskItem>();
            TasksDataGrid.ItemsSource = _tasks;
            Loaded += async (s, e) => await LoadData();
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            _searchText = SearchTextBox.Text.Trim();
            _ = LoadData();
        }
        private async Task LoadData()
        {
            try
            {
                var api = UserSession.Instance.Api;
                var currentUser = UserSession.Instance.CurrentUser;
                var myEmployeeId = currentUser.Employee?.IdEmployee;
                var myTeamId = currentUser.Employee?.Team?.IdTeam;
                var currentSprint = await api.GetActiveSprintAsync();

                List<TaskItem> tasks;

                if (myTeamId.HasValue)
                {
                    // Есть команда – получаем задачи всей команды + неназначенные
                    tasks = await api.GetTasksAsync(teamId: myTeamId.Value, includeUnassigned: true);
                }
                else
                {
                    // Нет команды – получаем свои задачи + неназначенные
                    tasks = await api.GetTasksAsync(assigneeId: myEmployeeId, includeUnassigned: true);
                }
                var filtered = tasks.Where(t => (t.Sprint == null || t.Sprint.Id == currentSprint.IdSprint) && t.Status?.Id != 5).ToList();

                if (!string.IsNullOrWhiteSpace(_searchText))
                {
                    if (int.TryParse(_searchText, out int taskId))
                    {
                        filtered = filtered.Where(t => t.IdTask == taskId).ToList();
                    }
                    else
                    {
                        filtered = filtered.Where(t => t.Title.IndexOf(_searchText, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
                    }
                }
                _tasks.Clear();
                foreach (var t in filtered)
                    _tasks.Add(t);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки задач: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Кнопка "Детали" – открыть окно деталей задачи.
        /// </summary>
        private void DetailsButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is TaskItem task)
            {
                var detailsWindow = new TaskDetailWindow(task);
                detailsWindow.ShowDialog();
                if (detailsWindow.is_edited)
                    _ = LoadData();
            }
        }

    }
}
