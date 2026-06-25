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

namespace TaskPlannerClient.Views.Tabs.Common
{
    /// <summary>
    /// Логика взаимодействия для EisenhowerMatrixTab.xaml
    /// </summary>
    public partial class EisenhowerMatrixTab : UserControl
    {
        // Для внешнего фильтра (из канбана по двойному клику на группу)
        private int? _externalStatusFilter;

        public EisenhowerMatrixTab()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        /// <summary>
        /// Вызывается из канбан-доски при двойном клике на группу,
        /// чтобы открыть матрицу с предустановленным фильтром по статусу.
        /// </summary>
        public void ApplyExternalStatusFilter(int? statusId)
        {
            _externalStatusFilter = statusId;
            // Если контрол уже загружен – перезагружаем данные
            if (IsLoaded)
                _ = LoadDataAsync();
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            await LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            try
            {
                await DataCache.LoadReferencesAsync();

                var api = UserSession.Instance.Api;
                var currentUser = UserSession.Instance.CurrentUser;
                string role = currentUser.Role;
                

                // Загружаем все задачи
                List<TaskItem> tasks = await api.GetTasksAsync();
                List<TaskItem> tasksFinal = new List<TaskItem>(tasks);

                // Фильтрация по роли
                if (role == "worker")
                {
                    var currentSprint = await api.GetActiveSprintAsync();
                    var myEmployeeId = currentUser.Employee?.IdEmployee;
                    if (myEmployeeId != null)
                    {
                        tasksFinal = tasks.Where(t => t.Assignee != null
                                && t.Assignee.Id == myEmployeeId && (t.Sprint == null || t.Sprint.Id == currentSprint.IdSprint))
                    .ToList();
                    }
                }
                // Если передан внешний фильтр по статусу (из канбана)
                if (_externalStatusFilter.HasValue)
                {
                    tasksFinal = tasksFinal.Where(t => t.Status?.Id == _externalStatusFilter.Value).ToList();
                }
                else
                {
                    tasksFinal = tasksFinal.Where(t => t.Status?.Id != 5 && t.Status?.Id != 6).ToList();
                    //if (role == "worker")
                    //{
                    //    tasksFinal = tasksFinal.Where(t => t.Status?.Id == 1 || t.Status?.Id == 2).ToList();  // "в работе" и "новые"
                    //}
                    //else
                    //{
                    //    tasksFinal = tasksFinal.Where(t => t.Status?.Id != 5 && t.Status?.Id != 6).ToList();
                    //}
                }

                // Сортируем задачи: сначала по важности (вес приоритета убывает), потом по срочности (дедлайн раньше)
                var sortedAll = tasksFinal
                    .OrderByDescending(t => t.Priority?.Weight ?? 0)
                    .ThenBy(t => t.Deadline ?? DateTime.MaxValue)
                    .ToList();
                //sortedAll = sortedAll.Where(t => t.Status?.Id != 5).ToList();
                // Распределяем по квадрантам
                ImportantUrgentList.ItemsSource = new ObservableCollection<TaskItem>(
                    sortedAll.Where(t => IsImportant(t) && IsUrgent(t)));
                ImportantNotUrgentList.ItemsSource = new ObservableCollection<TaskItem>(
                    sortedAll.Where(t => IsImportant(t) && !IsUrgent(t)));
                NotImportantUrgentList.ItemsSource = new ObservableCollection<TaskItem>(
                    sortedAll.Where(t => !IsImportant(t) && IsUrgent(t)));
                NotImportantNotUrgentList.ItemsSource = new ObservableCollection<TaskItem>(
                    sortedAll.Where(t => !IsImportant(t) && !IsUrgent(t)));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки матрицы Эйзенхауэра: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool IsImportant(TaskItem task)
        {
            // Важные: Критичный (100) и Высокий (75) приоритеты
            var weight = task.Priority?.Weight ?? 0;
            return weight >= 75;
        }

        private bool IsUrgent(TaskItem task)
        {
            // Срочные: дедлайн в ближайшие 2 дня или уже просрочен
            if (!task.Deadline.HasValue)
                return false;
            return (task.Deadline.Value - DateTime.Now).TotalDays <= 2;
        }

        /// <summary>
        /// Двойной клик по задаче — открыть детали.
        /// </summary>
        private void TaskList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
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
