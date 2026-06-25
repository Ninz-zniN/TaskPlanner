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
using static TaskPlannerClient.Views.TaskEditWindow;

namespace TaskPlannerClient.Views.Tabs.Manager
{
    /// <summary>
    /// Логика взаимодействия для TasksManagementTab.xaml
    /// </summary>
    public partial class TasksManagementTab : UserControl
    {
        private List<Project> _projects;
        private List<ReferenceItem> _statuses;
        private List<Employee> _employees;
        private List<Sprint> _sprints;
        private string _searchText = "";

        public ObservableCollection<TaskItem> Tasks { get; set; } = new ObservableCollection<TaskItem>();

        public TasksManagementTab()
        {
            InitializeComponent();
            TasksDataGrid.ItemsSource = Tasks;
            Loaded += OnLoaded;
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            await LoadFiltersAsync();
            await LoadTasksAsync();
        }

        private async System.Threading.Tasks.Task LoadFiltersAsync()
        {
            var api = UserSession.Instance.Api;
            _projects = await DataCache.GetProjectsAsync(); //await api.GetProjectsAsync();
            _statuses = DataCache.TaskStatuses; //ReferenceCache.TaskStatuses;//await api.GetTaskStatusesAsync();
            _employees = await DataCache.GetEmployeesAsync(); //await api.GetEmployeesAsync();
            _sprints = await DataCache.GetSprintsAsync(); //await api.GetSprintsAsync();

            // Проекты
            var projectItems = new List<Project> { new Project { IdProject = 0, ProjectName = "Все" } };
            projectItems.AddRange(_projects);
            FilterProjectCombo.ItemsSource = projectItems;

            // Статусы
            var statusItems = new List<ReferenceItem> { new ReferenceItem { Id = 0, Name = "Все" } };
            statusItems.AddRange(_statuses);
            FilterStatusCombo.ItemsSource = statusItems;

            // Исполнители
            var assigneeItems = new List<Employee> { new Employee { IdEmployee = 0, LastName = "Все", FirstName = "" } };
            assigneeItems.AddRange(_employees);
            FilterAssigneeCombo.ItemsSource = assigneeItems;

            // Спринты
            var sprintItems = new List<Sprint> { new Sprint { IdSprint = 0, SprintName = "Все" } };
            sprintItems.AddRange(_sprints);
            FilterSprintCombo.ItemsSource = sprintItems;
        }

        private async System.Threading.Tasks.Task LoadTasksAsync()
        {
            try
            {
                var api = UserSession.Instance.Api;
                var tasks = await api.GetTasksAsync();
                Tasks.Clear();
                foreach (var t in tasks)
                    Tasks.Add(t);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки задач: {ex.Message}");
            }
        }

        private void Filter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void ClearFiltersButton_Click(object sender, RoutedEventArgs e)
        {
            FilterProjectCombo.SelectedIndex = 0;
            FilterStatusCombo.SelectedIndex = 0;
            FilterAssigneeCombo.SelectedIndex = 0;
            FilterSprintCombo.SelectedIndex = 0;
            ApplyFilters();
        }

        private async void ApplyFilters()
        {
            // Используем API с параметрами (сервер сам умеет фильтровать)
            int? projectId = FilterProjectCombo.SelectedValue as int?;
            int? statusId = FilterStatusCombo.SelectedValue as int?;
            int? assigneeId = FilterAssigneeCombo.SelectedValue as int?;
            int? sprintId = FilterSprintCombo.SelectedValue as int?;

            try
            {
                var api = UserSession.Instance.Api;
                var tasks = await api.GetTasksAsync(); // пока без фильтров, но можно добавить параметры
                // Фильтрация на клиенте (временно, пока mock не поддерживает все параметры)
                var filtered = tasks.AsEnumerable();
                if (projectId.HasValue && projectId.Value != 0)
                    filtered = filtered.Where(t => t.Project?.Id == projectId);
                if (statusId.HasValue && statusId.Value != 0)
                    filtered = filtered.Where(t => t.Status?.Id == statusId);
                if (assigneeId.HasValue && assigneeId.Value != 0)
                    filtered = filtered.Where(t => t.Assignee?.Id == assigneeId);
                if (sprintId.HasValue && sprintId.Value != 0)
                    filtered = filtered.Where(t => t.Sprint?.Id == sprintId);
                if (!string.IsNullOrWhiteSpace(_searchText))
                {
                    if (int.TryParse(_searchText, out int taskId))
                    {
                        filtered = filtered.Where(t => t.IdTask == taskId);
                    }
                    else
                    {
                        filtered = filtered.Where(t => t.Title.IndexOf(_searchText, StringComparison.OrdinalIgnoreCase) >= 0);
                    }
                }

                Tasks.Clear();
                foreach (var t in filtered)
                    Tasks.Add(t);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка фильтрации: {ex.Message}");
            }
        }

        private async void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            var window = new TaskEditWindow();
            window.Owner = Window.GetWindow(this);
            if (window.ShowDialog() == true)
                await LoadTasksAsync();
        }

        private async void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is TaskItem task)
            {
                var window = new TaskEditWindow(task);
                window.Owner = Window.GetWindow(this);
                if (window.ShowDialog() == true)
                    await LoadTasksAsync();
            }
        }

        private async void StatusButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is TaskItem task)
            {
                var window = new TaskEditWindow(task, statusOnly: true);
                window.Owner = Window.GetWindow(this);
                if (window.ShowDialog() == true)
                    await LoadTasksAsync();
            }
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is TaskItem task)
            {
                var result = MessageBox.Show($"Удалить задачу '{task.Title}'?", "Подтверждение",
                    MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        await UserSession.Instance.Api.DeleteTaskAsync(task.IdTask);
                        await LoadTasksAsync();
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
            await LoadTasksAsync();
        }

        private void KanbanButton_Click(object sender, RoutedEventArgs e)
        {
            // Получаем текущие значения фильтров, чтобы передать в канбан
            int? assigneeId = (FilterAssigneeCombo.SelectedItem as Employee)?.IdEmployee;
            if (assigneeId == 0) assigneeId = null;

            var kanban = new Common.KanbanTab();
            if (assigneeId.HasValue)
                kanban.SetAssigneeFilter(assigneeId.Value);

            var window = new Window
            {
                Title = "Канбан-доска",
                Content = kanban,
                Width = 1100,
                Height = 700,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = Window.GetWindow(this)
            };
            window.ShowDialog();
        }

        private void MatrixButton_Click(object sender, RoutedEventArgs e)
        {
            var matrix = new Views.Tabs.Common.EisenhowerMatrixTab();
            // Если в фильтре выбран конкретный статус – передаём его
            int? statusId = FilterStatusCombo.SelectedValue as int?;
            if (statusId == 0) statusId = null;
            if (statusId.HasValue)
                matrix.ApplyExternalStatusFilter(statusId.Value);

            var window = new Window
            {
                Title = "Матрица Эйзенхауэра",
                Content = matrix,
                Width = 1000,
                Height = 700,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = Window.GetWindow(this)
            };
            window.ShowDialog();
        }

        private void CalendarButton_Click(object sender, RoutedEventArgs e)
        {
            var calendar = new Views.Tabs.Common.DeadlineCalendarTab();
            var window = new Window
            {
                Title = "Календарь дедлайнов",
                Content = calendar,
                Width = 900,
                Height = 600,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = Window.GetWindow(this)
            };
            window.ShowDialog();
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            _searchText = SearchTextBox.Text.Trim();
            ApplyFilters();
        }
    }
}
