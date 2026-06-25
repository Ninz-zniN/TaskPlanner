using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
    /// Логика взаимодействия для KanbanTab.xaml
    /// </summary>
    public partial class KanbanTab : UserControl
    {
        private bool _isMiddleDragging = false;
        private Point _lastMiddlePosition;
        public bool ShowAssignee { get; set; } = true;
        private ObservableCollection<KanbanColumn> _columns;
        // Фильтр по исполнителю (опционально)
        private int? _filterAssigneeId;

        public KanbanTab()
        {
            InitializeComponent();
            _columns = new ObservableCollection<KanbanColumn>();
            ColumnsItemsControl.ItemsSource = _columns;
            Loaded += OnLoaded;
        }

        /// <summary>Позволяет установить фильтр по исполнителю при открытии извне.</summary>
        public void SetAssigneeFilter(int? assigneeId)
        {
            _filterAssigneeId = assigneeId;
            ShowAssignee = false;
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            await LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            try
            {
                var statuses = DataCache.TaskStatuses;
                var api = UserSession.Instance.Api;
                var currentUser = UserSession.Instance.CurrentUser;
                string role = currentUser.Role;
                var currSprint = await api.GetActiveSprintAsync();

                List<TaskItem> allTasks;

                if (role == "worker" || _filterAssigneeId.HasValue)
                {
                    // Worker или явно передан фильтр
                    int? assigneeId = _filterAssigneeId ?? currentUser.Employee?.IdEmployee;
                    if (assigneeId == null)
                    {
                        // Нет привязки – пусто
                        _columns.Clear();
                        return;
                    }

                    var activeTasks = await api.GetTasksAsync(assigneeId: assigneeId.Value, includeUnassigned: false);
                    var awaitingTasks = await api.GetTasksAsync(previousAssigneeId: assigneeId.Value);
                    awaitingTasks = awaitingTasks.Where(t => t.Assignee.Id != assigneeId).ToList();
                    foreach (var task in awaitingTasks) task.IsAwaiting = true;
                    allTasks = activeTasks.Union(awaitingTasks).ToList();
                    allTasks = allTasks.Where(t => (t.Sprint == null && t.Status.Id != 5) || t.Sprint?.Id == currSprint.IdSprint).ToList();
                }
                else
                {
                    // Manager / Admin – видим все задачи
                    allTasks = await api.GetTasksAsync();
                }

                var headerColors = new Dictionary<int, SolidColorBrush>
                {
                    { 1, new SolidColorBrush((Color)ColorConverter.ConvertFromString("#90CAF9")) },
                    { 2, new SolidColorBrush((Color)ColorConverter.ConvertFromString("#CE93D8")) },
                    { 3, new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFCC80")) },
                    { 4, new SolidColorBrush((Color)ColorConverter.ConvertFromString("#A5D6A7")) },
                    { 5, new SolidColorBrush((Color)ColorConverter.ConvertFromString("#B0BEC5")) },
                };

                _columns.Clear();
                foreach (var status in statuses.Where(s => s.Id != 6))
                {
                    var colTasks = allTasks.Where(t => t.Status?.Id == status.Id).ToList();
                    _columns.Add(new KanbanColumn
                    {
                        StatusId = status.Id,
                        StatusName = status.Name,
                        Tasks = new ObservableCollection<TaskItem>(colTasks),
                        HeaderBackground = headerColors.TryGetValue(status.Id, out var brush) ? brush : new SolidColorBrush(Colors.Transparent),
                        ShowAssignee = this.ShowAssignee
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки канбан-доски: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ================== Drag & Drop ==================
        private void TaskCard_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Ничего не делаем, событие нужно для захвата мыши
        }

        private void TaskCard_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && sender is ListBoxItem item && item.DataContext is TaskItem task)
            {
                DragDrop.DoDragDrop(item, task, DragDropEffects.Move);
            }
        }

        private void Column_Drop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(typeof(TaskItem))) return;

            var droppedTask = e.Data.GetData(typeof(TaskItem)) as TaskItem;
            if (droppedTask == null) return;

            // Определяем целевую колонку
            var listBox = sender as ListBox;
            if (listBox == null) return;

            // Находим KanbanColumn, которому принадлежит этот ListBox
            KanbanColumn targetColumn = null;
            foreach (var col in _columns)
            {
                // Определяем, является ли listBox частью текущей колонки (через ItemsSource)
                if (col.Tasks == listBox.ItemsSource)
                {
                    targetColumn = col;
                    break;
                }
            }
            if (targetColumn == null) return;

            // Если статус не изменился – ничего не делаем
            if (droppedTask.Status?.Id == targetColumn.StatusId) return;

            // Обновляем статус через API
            _ = UpdateTaskStatusAsync(droppedTask, targetColumn);
        }

        private async Task UpdateTaskStatusAsync(TaskItem task, KanbanColumn targetColumn)
        {
            try
            {
                var api = UserSession.Instance.Api;
                int newStatusId = targetColumn.StatusId;

                // Если админ – спрашиваем про автоматическую историю
                DateTime? manualDate = null;
                decimal? manualHours = null;
                if (UserSession.Instance.CurrentUser.Role == "admin")
                {
                    var result = MessageBox.Show("Сгенерировать историю автоматически?", "Изменение статуса",
                        MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result == MessageBoxResult.No)
                    {
                        var dialog = new ManualHistoryDialog();
                        dialog.Owner = Window.GetWindow(this);
                        if (dialog.ShowDialog() == true && dialog.SelectedDate.HasValue)
                        {
                            manualDate = dialog.SelectedDate.Value;
                            manualHours = dialog.ActualHours;
                        }
                    }
                }

                // Вызываем обновление статуса (с параметрами ручного ввода, если они заданы)
                var updatedTask = await api.UpdateTaskStatusAsync(task.IdTask, newStatusId, manualDate, manualHours);

                // Обновляем локально: удаляем из старой колонки и добавляем в новую
                var oldColumn = _columns.FirstOrDefault(c => c.Tasks.Contains(task));
                if (oldColumn != null)
                    oldColumn.Tasks.Remove(task);

                // Подменяем данные задачи на обновлённые
                task.Status = updatedTask.Status;
                task.UpdatedAt = updatedTask.UpdatedAt;

                targetColumn.Tasks.Add(task);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка перемещения задачи: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Открывает детали задачи по двойному клику.
        /// </summary>
        private void TaskCard_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is ListBoxItem item && item.DataContext is TaskItem task)
            {
                var detailsWindow = new TaskDetailWindow(task);
                detailsWindow.ShowDialog();
                if (detailsWindow.is_edited)
                    _ = LoadDataAsync();
            }
        }
        private void Header_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // Получаем колонку, по чьему заголовку кликнули
            if (sender is FrameworkElement element && element.DataContext is KanbanColumn column && column.StatusId != 5)
            {
                var mainWindow = Window.GetWindow(this) as MainWindow;
                if (mainWindow != null)
                {
                    mainWindow.OpenEisenhowerMatrix(column.StatusId);
                }
                else
                {
                    var matrix = new EisenhowerMatrixTab();
                    matrix.ApplyExternalStatusFilter(column.StatusId);
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
            }
        }
        //методы для прокрутки колесом
        private void MainScrollViewer_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Начинаем drag-скролл только при нажатии средней кнопки (колеса)
            if (e.MiddleButton == MouseButtonState.Pressed)
            {
                _isMiddleDragging = true;
                _lastMiddlePosition = e.GetPosition(MainScrollViewer);
                MainScrollViewer.CaptureMouse();
                e.Handled = true;
            }
        }

        private void MainScrollViewer_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isMiddleDragging)
            {
                Point currentPosition = e.GetPosition(MainScrollViewer);
                Vector delta = _lastMiddlePosition - currentPosition;
                MainScrollViewer.ScrollToHorizontalOffset(MainScrollViewer.HorizontalOffset + delta.X);
                _lastMiddlePosition = currentPosition;
                e.Handled = true;
            }
        }

        private void MainScrollViewer_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_isMiddleDragging && e.MiddleButton == MouseButtonState.Released)
            {
                _isMiddleDragging = false;
                MainScrollViewer.ReleaseMouseCapture();
                e.Handled = true;
            }
        }

        
    }

    // ================== Вспомогательный класс ==================
    public class KanbanColumn
    {
        public int StatusId { get; set; }
        public string StatusName { get; set; }
        public ObservableCollection<TaskItem> Tasks { get; set; }
        public Brush HeaderBackground { get; set; }
        public bool ShowAssignee { get; set; }
    }
}
