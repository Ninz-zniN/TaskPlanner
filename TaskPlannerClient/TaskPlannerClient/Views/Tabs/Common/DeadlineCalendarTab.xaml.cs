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
using TaskPlannerClient.Converters;
using TaskPlannerClient.Models;
using TaskPlannerClient.Service;

namespace TaskPlannerClient.Views.Tabs.Common
{
    /// <summary>
    /// Логика взаимодействия для DeadlineCalendarTab.xaml
    /// </summary>
    public partial class DeadlineCalendarTab : UserControl
    {
        private Dictionary<DateTime, List<TaskItem>> _tasksByDate = new();
        private List<TaskItem> _allTasks;

        /// <summary>Показывать ли исполнителя в карточках (для Manager/Admin).</summary>
        public bool ShowAssignee { get; set; } = true;

        public DeadlineCalendarTab()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            string role = UserSession.Instance.CurrentUser.Role;
            ShowAssignee = (role == "project_manager" || role == "admin");

            await LoadDataAsync();
            UpdateConverter();
        }

        /// <summary>
        /// Загружает задачи с фильтрацией как в матрице.
        /// </summary>
        private async Task LoadDataAsync()
        {
            try
            {
                var api = UserSession.Instance.Api;
                var currentUser = UserSession.Instance.CurrentUser;
                string role = currentUser.Role;

                List<TaskItem> tasks;
                if (role == "worker")
                {
                    var currentSprint = await api.GetActiveSprintAsync();
                    var myEmployeeId = currentUser.Employee?.IdEmployee;
                    if (myEmployeeId == null)
                    {
                        _tasksByDate = new();
                        return;
                    }
                    tasks = await api.GetTasksAsync(assigneeId: myEmployeeId.Value, includeUnassigned: false);
                    // Только "Новая" (1) и "В работе" (2)
                    //tasks = tasks.Where(t => t.Status?.Id == 1 || t.Status?.Id == 2).ToList();
                    tasks = tasks.Where(t => (t.Sprint == null || t.Sprint.Id == currentSprint.IdSprint) && t.Status?.Id != 5 && t.Status?.Id != 6).ToList();
                }
                else
                {
                    tasks = await api.GetTasksAsync();
                    // Исключаем "Готово" (5) и "Отменена" (6)
                    tasks = tasks.Where(t => t.Status?.Id != 5 && t.Status?.Id != 6).ToList();
                }

                // Группируем по дате дедлайна
                _tasksByDate = tasks
                    .Where(t => t.Deadline.HasValue)
                    .GroupBy(t => t.Deadline.Value.Date)
                    .ToDictionary(g => g.Key, g => g.ToList());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateConverter()
        {
            if (FindResource("HasDeadlineConverter") is Converters.HasDeadlineConverter conv)
                conv.TasksByDate = _tasksByDate;
        }

        private void DeadlineCalendar_SelectedDatesChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DeadlineCalendar.SelectedDate.HasValue)
            {
                DateTime selected = DeadlineCalendar.SelectedDate.Value.Date;
                SelectedDateText.Text = selected.ToString("dd MMMM yyyy");

                if (_tasksByDate.TryGetValue(selected, out var tasks))
                {
                    TasksListBox.ItemsSource = new ObservableCollection<TaskItem>(tasks);
                }
                else
                {
                    TasksListBox.ItemsSource = null;
                }
            }
        }

        private void TasksListBox_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is ListBox listBox && listBox.SelectedItem is TaskItem task)
            {
                var detailsWindow = new TaskDetailWindow(task);
                detailsWindow.ShowDialog();
                if (detailsWindow.is_edited)
                    _ = LoadDataAsync();
            }
        }
    }
}
