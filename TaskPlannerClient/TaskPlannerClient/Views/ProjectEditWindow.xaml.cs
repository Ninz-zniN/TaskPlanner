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
using static MaterialDesignThemes.Wpf.Theme.ToolBar;

namespace TaskPlannerClient.Views
{
    /// <summary>
    /// Логика взаимодействия для ProjectEditWindow.xaml
    /// </summary>
    public partial class ProjectEditWindow : Window
    {
        private List<Team> _teams;
        private readonly Project? _existingProject;
        private List<Employee> _employees;

        public ProjectEditWindow(Project? project = null)
        {
            InitializeComponent();
            _existingProject = project;
            Loaded += OnLoaded;
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                _employees = await DataCache.GetEmployeesAsync(); //await UserSession.Instance.Api.GetEmployeesAsync();
                string role = UserSession.Instance.CurrentUser.Role;
                bool isManager = role == "project_manager";
                Employee? myEmployee = UserSession.Instance.CurrentUser.Employee;
                _teams = await DataCache.GetTeamsAsync();

                // Формируем список в зависимости от роли
                if (isManager && _existingProject == null)
                {
                    // Менеджер создаёт проект: только я сам или "не выбран"
                    var items = new List<Employee> { new Employee { IdEmployee = -1, LastName = "Не выбран", FirstName = "", Position = "" } };
                    if (myEmployee != null)
                        items.Add(myEmployee);
                    ManagerComboBox.ItemsSource = items;
                    ManagerComboBox.IsEnabled = true;
                }
                else
                {
                    // Админ или редактирование: полный список
                    var managers = new List<Employee> { new Employee { IdEmployee = -1, LastName = "Не выбран", FirstName = "", Position = "" } };
                    managers.AddRange(_employees);
                    ManagerComboBox.ItemsSource = managers;

                    // Блокируем менеджеру смену менеджера при редактировании
                    if (isManager)
                        ManagerComboBox.IsEnabled = false;
                    else
                        ManagerComboBox.IsEnabled = true;
                }

                // Команды
                var teams = new List<Team> { new Team { IdTeam = -1, TeamName = "Не выбрана" } };
                teams.AddRange(_teams);
                TeamComboBox.ItemsSource = teams;

                // Заполняем поля, если редактируем
                if (_existingProject != null)
                {
                    ProjectNameTextBox.Text = _existingProject.ProjectName;
                    DescriptionTextBox.Text = _existingProject.Description ?? "";
                    StatusComboBox.SelectedValue = _existingProject.Status;

                    ManagerComboBox.SelectedValue = _existingProject.Manager?.Id ?? -1;
                    TeamComboBox.SelectedValue = _existingProject.Team?.IdTeam ?? -1;

                    if (_existingProject.Manager != null)
                        ManagerComboBox.SelectedValue = _existingProject.Manager.Id;
                    else
                        ManagerComboBox.SelectedValue = -1; // "Не выбран"
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

                string name = ProjectNameTextBox.Text.Trim();
                if (string.IsNullOrWhiteSpace(name))
                {
                    MessageBox.Show("Название проекта обязательно.");
                    return;
                }

                string? status_ = (StatusComboBox.SelectedItem as ComboBoxItem)?.Tag as string;
                int? managerId = ManagerComboBox.SelectedValue as int?;
                if (managerId == -1) managerId = null;
                int? teamId = (TeamComboBox.SelectedItem as Team)?.IdTeam;
                if (teamId == -1) teamId = null;

                var api = UserSession.Instance.Api;

                if (_existingProject == null)
                {
                    var dto = new CreateProjectDto(
                        projectName: name,
                        description: DescriptionTextBox.Text,
                        status: status_,
                        idManager: managerId,
                        idTeam: teamId
                    );
                    await api.CreateProjectAsync(dto);
                }
                else
                {
                    var dto = new ProjectUpdateDto
                    {
                        ProjectName = name,
                        Description = DescriptionTextBox.Text,
                        Status = status_,
                        IdManager = managerId,
                        IdTeam = teamId
                    };
                    await api.UpdateProjectAsync(_existingProject.IdProject, dto);
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
    }
}
