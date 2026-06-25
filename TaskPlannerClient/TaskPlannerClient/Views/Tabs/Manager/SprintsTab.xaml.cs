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
    /// Логика взаимодействия для SprintsTab.xaml
    /// </summary>
    public partial class SprintsTab : UserControl
    {
        public ObservableCollection<Sprint> Sprints { get; set; } = new ObservableCollection<Sprint>();

        public SprintsTab()
        {
            InitializeComponent();
            SprintsDataGrid.ItemsSource = Sprints;
            Loaded += async (s, e) => await LoadSprints();
        }

        private async System.Threading.Tasks.Task LoadSprints()
        {
            try
            {
                var sprints = await DataCache.GetSprintsAsync(true); //await UserSession.Instance.Api.GetSprintsAsync();
                Sprints.Clear();
                foreach (var s in sprints)
                    Sprints.Add(s);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки спринтов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            var window = new SprintEditWIndow();
            window.Owner = Window.GetWindow(this);
            if (window.ShowDialog() == true)
                await LoadSprints();
        }

        private async void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is Sprint sprint)
            {
                var window = new SprintEditWIndow(sprint);
                window.Owner = Window.GetWindow(this);
                if (window.ShowDialog() == true)
                    await LoadSprints();
            }
        }

        private async void ActivateButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is Sprint sprint)
            {
                try
                {
                    // Пытаемся активировать новый спринт
                    await UserSession.Instance.Api.ActivateSprintAsync(sprint.IdSprint);
                    await LoadSprints();
                }
                catch (Exception ex)
                {
                    // Если сервер вернул 409 (есть незавершённые задачи), показываем диалог закрытия
                    if (ex.Message.Contains("409"))
                    {
                        // Получаем информацию о текущем активном спринте
                        var activeSprint = Sprints.FirstOrDefault(s => s.IsActive);
                        if (activeSprint != null)
                        {
                            var dialog = new SprintCloseDialog();
                            dialog.Owner = Window.GetWindow(this);
                            if (dialog.ShowDialog() == true)
                            {
                                string action = dialog.SelectedAction;
                                try
                                {
                                    // Закрываем текущий активный спринт с выбранным действием
                                    await UserSession.Instance.Api.CloseSprintAsync(activeSprint.IdSprint, action);
                                    // Теперь пробуем активировать новый спринт снова
                                    await UserSession.Instance.Api.ActivateSprintAsync(sprint.IdSprint);
                                    await LoadSprints();
                                }
                                catch (Exception closeEx)
                                {
                                    MessageBox.Show($"Ошибка закрытия спринта: {closeEx.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                                }
                            }
                        }
                        else
                        {
                            MessageBox.Show("Нет активного спринта для закрытия.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                    else
                    {
                        MessageBox.Show($"Ошибка активации: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private async void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is Sprint sprint)
            {
                // Диалог выбора действия
                var dialog = new TaskPlannerClient.Views.SprintCloseDialog();
                dialog.Owner = Window.GetWindow(this);
                if (dialog.ShowDialog() == true)
                {
                    string action = dialog.SelectedAction; // "move_to_next", "move_to_backlog", "cancel_tasks"
                    try
                    {
                        await UserSession.Instance.Api.CloseSprintAsync(sprint.IdSprint, action);
                        await LoadSprints();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка закрытия спринта: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is Sprint sprint)
            {
                var result = MessageBox.Show($"Удалить спринт '{sprint.SprintName}'?", "Подтверждение",
                    MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        await UserSession.Instance.Api.DeleteSprintAsync(sprint.IdSprint);
                        await LoadSprints();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка удаления: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            await LoadSprints();
        }
    }
}
