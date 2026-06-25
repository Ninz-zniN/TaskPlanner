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
    /// Логика взаимодействия для EmployeeEditWindow.xaml
    /// </summary>
    public partial class EmployeeEditWindow : Window
    {
        private readonly Employee? _existingEmployee;
        private List<Team> _teams;

        public EmployeeEditWindow(Employee? employee = null)
        {
            InitializeComponent();
            _existingEmployee = employee;
            Loaded += OnLoaded;
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var teams = await DataCache.GetTeamsAsync(); //await UserSession.Instance.Api.GetTeamsAsync();
                _teams = teams;
                var teamItems = new List<Team> { new Team { IdTeam = -1, TeamName = "Не выбрана" } };
                teamItems.AddRange(teams);
                TeamComboBox.ItemsSource = teamItems;

                if (_existingEmployee != null)
                {
                    LastNameTextBox.Text = _existingEmployee.LastName;
                    FirstNameTextBox.Text = _existingEmployee.FirstName;
                    PatronymicTextBox.Text = _existingEmployee.Patronymic ?? "";
                    PositionTextBox.Text = _existingEmployee.Position ?? "";
                    GradeComboBox.SelectedValue = _existingEmployee.Grade ?? "Middle";
                    HoursTextBox.Text = _existingEmployee.HoursPerDay.ToString();
                    TeamComboBox.SelectedValue = _existingEmployee.Team?.IdTeam ?? -1;
                }
                else
                {
                    GradeComboBox.SelectedValue = "Middle";
                    HoursTextBox.Text = "8";
                    TeamComboBox.SelectedValue = -1;
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

                string lastName = LastNameTextBox.Text.Trim();
                string firstName = FirstNameTextBox.Text.Trim();
                if (string.IsNullOrWhiteSpace(lastName) || string.IsNullOrWhiteSpace(firstName))
                {
                    MessageBox.Show("Фамилия и имя обязательны.");
                    return;
                }

                if (!decimal.TryParse(HoursTextBox.Text, out decimal hours) || hours <= 0)
                {
                    MessageBox.Show("Некорректное количество часов.");
                    return;
                }

                int? teamId = TeamComboBox.SelectedValue as int?;
                if (teamId == -1) teamId = null;
                string? grade = GradeComboBox.SelectedValue as string;

                var api = UserSession.Instance.Api;

                if (_existingEmployee == null)
                {
                    var dto = new CreateEmployeeDto(
                        lastName: lastName,
                        firstName: firstName,
                        patronymic: PatronymicTextBox.Text.Trim(),
                        position: PositionTextBox.Text.Trim(),
                        grade: grade,
                        idTeam: teamId,
                        hoursPerDay: hours
                    );
                    await api.CreateEmployeeAsync(dto);
                }
                else
                {
                    var dto = new EmployeeUpdateDto
                    {
                        LastName = lastName,
                        FirstName = firstName,
                        Patronymic = PatronymicTextBox.Text.Trim(),
                        Position = PositionTextBox.Text.Trim(),
                        Grade = grade,
                        IdTeam = teamId,
                        HoursPerDay = hours
                    };
                    await api.UpdateEmployeeAsync(_existingEmployee.IdEmployee, dto);
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
