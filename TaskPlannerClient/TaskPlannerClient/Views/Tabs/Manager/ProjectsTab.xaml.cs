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
    /// Логика взаимодействия для ProjectsTab.xaml
    /// </summary>
    public partial class ProjectsTab : UserControl
    {
        public bool IsAdmin => UserSession.Instance.CurrentUser.Role == "admin";
        public ObservableCollection<Project> Projects { get; set; } = new ObservableCollection<Project>();

        public ProjectsTab()
        {
            InitializeComponent();
            ProjectsDataGrid.ItemsSource = Projects;
            Loaded += async (s, e) => await LoadProjects();
        }

        private async System.Threading.Tasks.Task LoadProjects()
        {
            try
            {
                var projects = await DataCache.GetProjectsAsync(true); //await UserSession.Instance.Api.GetProjectsAsync();
                Projects.Clear();
                foreach (var p in projects)
                    Projects.Add(p);//
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки проектов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void CompleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is Project project)
            {
                var result = MessageBox.Show($"Завершить проект '{project.ProjectName}'?", "Подтверждение",
                    MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        await UserSession.Instance.Api.CompleteProjectAsync(project.IdProject);
                        await LoadProjects();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка завершения проекта: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private async void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            var window = new ProjectEditWindow();   // без параметров – создание
            window.Owner = Window.GetWindow(this);
            if (window.ShowDialog() == true)
                await LoadProjects();
        }

        private async void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is Project project)
            {
                var window = new ProjectEditWindow(project);   // редактирование
                window.Owner = Window.GetWindow(this);
                if (window.ShowDialog() == true)
                    await LoadProjects();
            }
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is Project project)
            {
                var result = MessageBox.Show($"Удалить проект '{project.ProjectName}'?", "Подтверждение",
                    MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        await UserSession.Instance.Api.DeleteProjectAsync(project.IdProject);
                        await LoadProjects();
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
            await LoadProjects();
        }
    }
}
