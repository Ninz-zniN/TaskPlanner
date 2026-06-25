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
    /// Логика взаимодействия для TaskDetailWindow.xaml
    /// </summary>
    public partial class TaskDetailWindow : Window
    {
        public bool is_edited = false;
        private readonly TaskItem _task;
        private List<ReferenceItem> _statuses;

        // Права (только для Worker)
        private bool _canEditStatus;      // менять статус своей задачи
        private bool _canAddNotes;        // добавлять заметки
        private bool _canTake;            // брать задачу

        public TaskDetailWindow(TaskItem task)
        {
            InitializeComponent();
            _task = task;
            Loaded += async (s, e) => await LoadData();
        }

        private async System.Threading.Tasks.Task LoadData()
        {
            try
            {
                var currentUser = UserSession.Instance.CurrentUser;
                var myEmployeeId = currentUser.Employee?.IdEmployee;

                // Загружаем справочник статусов
                _statuses = DataCache.TaskStatuses;
                StatusComboBox.ItemsSource = _statuses;

                // Заполняем поля (только чтение)
                TitleText.Text = _task.Title;
                TypeText.Text = _task.TaskType?.Name;
                PriorityText.Text = $"{_task.Priority?.Name} (вес {_task.Priority?.Weight})";
                EstimateText.Text = _task.EstimateHours.ToString();
                ActualText.Text = _task.ActualHours?.ToString() ?? "—";
                DeadlineText.Text = _task.Deadline?.ToString("dd.MM.yyyy") ?? "—";
                CreatedAtText.Text = _task.CreatedAt.ToString("dd.MM.yyyy HH:mm");
                UpdatedAtText.Text = _task.UpdatedAt?.ToString("dd.MM.yyyy HH:mm") ?? "—";
                AssigneeText.Text = _task.Assignee != null ? $"{_task.Assignee.LastName} {_task.Assignee.FirstName}" : "—";
                ProjectText.Text = _task.Project?.Name;
                SprintText.Text = _task.Sprint?.Name ?? "—";
                DescriptionTextBox.Text = _task.Description ?? "";

                // Определяем права Worker'а
                if ((myEmployeeId == null || _task.IsAwaiting) && currentUser.Role == "worker")
                {
                    _canEditStatus = false;
                    _canTake = false;
                    _canAddNotes = false;
                }
                else if (_task.Assignee == null && currentUser.Role == "worker")
                {
                    // Свободная задача – можно взять
                    _canEditStatus = false;
                    _canTake = true;
                    _canAddNotes = false;   // заметки только для своих задач
                }
                else if (currentUser.Role != "worker" || _task.Assignee?.Id == myEmployeeId.Value)
                {
                    _canEditStatus = true;
                    _canTake = false;
                    _canAddNotes = true;
                }
                else
                {
                    _canEditStatus = false;
                    _canTake = false;
                    _canAddNotes = false;
                }

                // Применяем права
                StatusComboBox.Visibility = _canEditStatus ? Visibility.Visible : Visibility.Collapsed;
                SaveButton.Visibility = _canEditStatus ? Visibility.Visible : Visibility.Collapsed;
                TakeButton.Visibility = _canTake ? Visibility.Visible : Visibility.Collapsed;
                DescriptionTextBox.IsEnabled = false;
                AddNoteButton.Visibility = _canAddNotes ? Visibility.Visible : Visibility.Collapsed;
                NoteTextBox.Visibility = _canAddNotes ? Visibility.Visible : Visibility.Collapsed;
                EditButton.Visibility = _canEditStatus ? Visibility.Visible : Visibility.Collapsed;

                if (_canEditStatus)
                    StatusComboBox.SelectedValue = _task.Status?.Id;

                // Загружаем заметки
                await LoadNotes();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}");
            }
        }

        private async System.Threading.Tasks.Task LoadNotes()
        {
            try
            {
                var notes = await UserSession.Instance.Api.GetTaskNotesAsync(_task.IdTask);
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
                await UserSession.Instance.Api.AddTaskNoteAsync(_task.IdTask, content);
                NoteTextBox.Text = "";
                await LoadNotes();
                is_edited = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка добавления заметки: {ex.Message}");
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            var editWindow = new TaskEditWindow(_task);
            editWindow.Owner = this;
            // Закрываем детали перед открытием редактора
            this.Hide();
            //this.DialogResult = editWindow.ShowDialog();
            editWindow.ShowDialog();
            is_edited = true;
            //this.DialogResult = true;
            this.Close();
        }

        private async void DeleteNoteButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is TaskNote note)
            {
                try
                {
                    await UserSession.Instance.Api.DeleteTaskNoteAsync(note.IdNote);
                    await LoadNotes();
                    is_edited = true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка удаления заметки: {ex.Message}");
                }
            }
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveButton.IsEnabled = false;
                int? newStatusId = StatusComboBox.SelectedValue as int?;
                if (newStatusId.HasValue && newStatusId != _task.Status?.Id)
                {
                    await UserSession.Instance.Api.UpdateTaskStatusAsync(_task.IdTask, newStatusId.Value);
                    _task.Status = _statuses.First(s => s.Id == newStatusId.Value);
                }

                MessageBox.Show("Изменения сохранены.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                is_edited = true;
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

        private async void TakeButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                TakeButton.IsEnabled = false;
                var myEmployeeId = UserSession.Instance.CurrentUser.Employee?.IdEmployee;
                if (myEmployeeId == null)
                {
                    MessageBox.Show("Нет привязки к сотруднику.");
                    return;
                }

                var updatedTask = await UserSession.Instance.Api.AssignTaskAsync(_task.IdTask, myEmployeeId.Value);

                // Обновляем локальные данные, чтобы при возврате списки обновились
                _task.Assignee = updatedTask.Assignee;

                MessageBox.Show("Задача назначена на вас.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
            finally
            {
                TakeButton.IsEnabled = true;
            }
        }
    }
}
