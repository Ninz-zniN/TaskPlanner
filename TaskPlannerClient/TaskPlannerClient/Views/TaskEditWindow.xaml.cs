using System;
using System.Collections.Generic;
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
using TaskPlannerClient.Models.Dto;
using TaskPlannerClient.Service;

namespace TaskPlannerClient.Views
{
    /// <summary>
    /// Логика взаимодействия для TaskEditWindow.xaml
    /// </summary>
    public partial class TaskEditWindow : Window
    {
        private readonly TaskItem? _existingTask;
        private readonly bool _statusOnly;
        private List<ReferenceItem> _taskTypes;
        private List<PriorityItem> _priorities;
        private List<Employee> _employees;
        private List<Project> _projects;
        private List<Sprint> _sprints;
        private List<ReferenceItem> _statuses;

        /// <summary>
        /// statusOnly = true – окно открыто только для смены статуса (остальные поля заблокированы)
        /// </summary>
        public TaskEditWindow(TaskItem? task = null, bool statusOnly = false)
        {
            InitializeComponent();
            _existingTask = task;
            _statusOnly = statusOnly;
            Loaded += OnLoaded;
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var api = UserSession.Instance.Api;
                _taskTypes = DataCache.TaskTypes; //await api.GetTaskTypesAsync();
                _priorities = DataCache.Priorities; //await api.GetPrioritiesAsync();
                _employees = await DataCache.GetEmployeesAsync(); //await api.GetEmployeesAsync();
                _projects = await DataCache.GetProjectsAsync(); //await api.GetProjectsAsync();
                _sprints = await DataCache.GetSprintsAsync(); //await api.GetSprintsAsync();
                _statuses = DataCache.TaskStatuses;//await api.GetTaskStatusesAsync();

                // Справочники
                TypeComboBox.ItemsSource = _taskTypes;
                PriorityComboBox.ItemsSource = _priorities;
                StatusComboBox.ItemsSource = _statuses;

                // Исполнители (с заглушкой)
                var assigneeItems = new List<Employee> { new Employee { IdEmployee = -1, LastName = "Не выбран", FirstName = "" } };
                assigneeItems.AddRange(_employees);
                AssigneeComboBox.ItemsSource = assigneeItems;

                // Проекты (с заглушкой)
                var projectItems = new List<Project> { new Project { IdProject = -1, ProjectName = "Не выбран" } };
                projectItems.AddRange(_projects);
                ProjectComboBox.ItemsSource = projectItems;

                // Спринты (с заглушкой)
                var sprintItems = new List<Sprint> { new Sprint { IdSprint = -1, SprintName = "Не выбран" } };
                sprintItems.AddRange(_sprints);
                SprintComboBox.ItemsSource = sprintItems;

                // Блокировка полей для statusOnly
                if (_statusOnly)
                {
                    TitleTextBox.IsEnabled = false;
                    TypeComboBox.IsEnabled = false;
                    PriorityComboBox.IsEnabled = false;
                    EstimateTextBox.IsEnabled = false;
                    DeadlinePicker.IsEnabled = false;
                    DescriptionTextBox.IsEnabled = false;
                    AssigneeComboBox.IsEnabled = false;
                    ProjectComboBox.IsEnabled = false;
                    SprintComboBox.IsEnabled = false;
                }

                // Загрузка данных задачи (если редактируем)
                if (_existingTask != null)
                {
                    TitleTextBox.Text = _existingTask.Title;
                    TypeComboBox.SelectedValue = _existingTask.TaskType?.Id;
                    PriorityComboBox.SelectedValue = _existingTask.Priority?.Id;
                    EstimateTextBox.Text = _existingTask.EstimateHours.ToString();
                    DeadlinePicker.SelectedDate = _existingTask.Deadline;
                    DescriptionTextBox.Text = _existingTask.Description ?? "";
                    AssigneeComboBox.SelectedValue = _existingTask.Assignee?.Id ?? -1;
                    ProjectComboBox.SelectedValue = _existingTask.Project?.Id ?? -1;
                    SprintComboBox.SelectedValue = _existingTask.Sprint?.Id ?? -1;
                    StatusComboBox.SelectedValue = _existingTask.Status?.Id;
                    await LoadNotes();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}");
            }
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveButton.IsEnabled = false;
                var api = UserSession.Instance.Api;

                if (_statusOnly && _existingTask != null)
                {
                    // Меняем только статус
                    int? newStatusId = StatusComboBox.SelectedValue as int?;
                    if (newStatusId.HasValue)
                    {
                        await api.UpdateTaskStatusAsync(_existingTask.IdTask, newStatusId.Value);
                    }
                }
                else
                {
                    string title = TitleTextBox.Text.Trim();
                    if (string.IsNullOrWhiteSpace(title))
                    {
                        MessageBox.Show("Название обязательно.");
                        return;
                    }

                    if (!decimal.TryParse(EstimateTextBox.Text, out decimal estimate) || estimate < 0)
                    {
                        MessageBox.Show("Некорректная оценка.");
                        return;
                    }

                    string? deadlineStr = DeadlinePicker.SelectedDate?.ToString("yyyy-MM-dd");
                    int? typeId = TypeComboBox.SelectedValue as int?;
                    int? priorityId = PriorityComboBox.SelectedValue as int?;
                    int? assigneeId = AssigneeComboBox.SelectedValue as int?;
                    int? projectId = ProjectComboBox.SelectedValue as int?;
                    int? sprintId = SprintComboBox.SelectedValue as int?;
                    int? statusId = StatusComboBox.SelectedValue as int?;

                    // Преобразуем -1 (заглушка) в null
                    if (assigneeId == -1) assigneeId = null;
                    if (projectId == -1) projectId = null;
                    if (sprintId == -1) sprintId = null;
                    string? description = DescriptionTextBox.Text.Trim();
                    if (string.IsNullOrEmpty(description)) description = null;

                    if (_existingTask == null)
                    {
                        // Создание
                        var dto = new CreateTaskDto(
                            title: title,
                            description: description,
                            idTaskType: typeId,
                            idPriority: priorityId,
                            estimateHours: estimate,
                            deadline: deadlineStr,
                            idAssignee: assigneeId,
                            idProject: projectId,
                            idSprint: sprintId,
                            idTaskStatus: StatusComboBox.SelectedValue as int?
                        );
                        await api.CreateTaskAsync(dto);
                    }
                    else
                    {
                        // Редактирование
                        var dto = new TaskUpdateDto
                        {
                            Title = title,
                            Description = description,
                            IdTaskType = typeId,
                            IdPriority = priorityId,
                            EstimateHours = estimate,
                            Deadline = deadlineStr,
                            IdAssignee = assigneeId,
                            IdSprint = sprintId,
                            //IdTaskStatus = StatusComboBox.SelectedValue as int?
                        };
                        await api.UpdateTaskAsync(_existingTask.IdTask, dto);

                        int? newStatusId = StatusComboBox.SelectedValue as int?;
                        if (newStatusId.HasValue && newStatusId != _existingTask.Status?.Id)
                        {
                            DateTime? manualDate = null;
                            decimal? manualHours = null;

                            if (UserSession.Instance.CurrentUser.Role == "admin")
                            {
                                var result = MessageBox.Show("Сгенерировать историю автоматически?", "Изменение статуса",
                                    MessageBoxButton.YesNo, MessageBoxImage.Question);
                                if (result == MessageBoxResult.No)
                                {
                                    var dialog = new ManualHistoryDialog();
                                    dialog.Owner = this;
                                    if (dialog.ShowDialog() == true && dialog.SelectedDate.HasValue)
                                    {
                                        manualDate = dialog.SelectedDate.Value;
                                        manualHours = dialog.ActualHours;
                                    }
                                }
                            }
                            await api.UpdateTaskStatusAsync(_existingTask.IdTask, newStatusId.Value, manualDate, manualHours);
                        }

                    }
                }

                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}");
            }
            finally
            {
                SaveButton.IsEnabled = true;
            }
        }

        private async System.Threading.Tasks.Task LoadNotes()
        {
            try
            {
                var notes = await UserSession.Instance.Api.GetTaskNotesAsync(_existingTask.IdTask);
                NotesItemsControl.ItemsSource = notes;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки заметок: {ex.Message}");
            }
        }

        private async void AddNoteButton_Click(object sender, RoutedEventArgs e)
        {
            var content = NoteTextBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(content))
            {
                MessageBox.Show("Введите текст заметки.");
                return;
            }

            try
            {
                await UserSession.Instance.Api.AddTaskNoteAsync(_existingTask.IdTask, content);
                NoteTextBox.Text = "";
                await LoadNotes();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка добавления заметки: {ex.Message}");
            }
        }

        private async void DeleteNoteButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is TaskNote note)
            {
                try
                {
                    await UserSession.Instance.Api.DeleteTaskNoteAsync(note.IdNote);
                    await LoadNotes();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка удаления заметки: {ex.Message}");
                }
            }
        }
    }
}
