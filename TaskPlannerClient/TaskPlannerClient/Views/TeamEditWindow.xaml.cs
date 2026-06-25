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
    /// Логика взаимодействия для TeamEditWindow.xaml
    /// </summary>
    public partial class TeamEditWindow : Window
    {
        private readonly Team? _existingTeam;
        private List<Employee> _employees;

        public TeamEditWindow(Team? team = null)
        {
            InitializeComponent();
            _existingTeam = team;
            Loaded += OnLoaded;
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var api = UserSession.Instance.Api;
                _employees = await DataCache.GetEmployeesAsync(); //await api.GetEmployeesAsync();

                var leadItems = new List<Employee> { new Employee { IdEmployee = -1, LastName = "Не выбран", FirstName = "", Position = "" } };
                leadItems.AddRange(_employees);
                TeamLeadComboBox.ItemsSource = leadItems;

                if (_existingTeam != null)
                {
                    TeamNameTextBox.Text = _existingTeam.TeamName;
                    if (_existingTeam.TeamLead != null)
                        TeamLeadComboBox.SelectedValue = _existingTeam.TeamLead.IdEmployee;
                    else
                        TeamLeadComboBox.SelectedValue = -1;
                }
                else
                {
                    TeamLeadComboBox.SelectedValue = -1;
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
                string name = TeamNameTextBox.Text.Trim();
                if (string.IsNullOrWhiteSpace(name))
                {
                    MessageBox.Show("Название команды обязательно.");
                    return;
                }

                int? leadId = TeamLeadComboBox.SelectedValue as int?;
                if (leadId == -1) leadId = null;

                var api = UserSession.Instance.Api;
                if (_existingTeam == null)
                {
                    var dto = new CreateTeamDto(name, leadId);
                    await api.CreateTeamAsync(dto);
                }
                else
                {
                    var dto = new TeamUpdateDto
                    {
                        TeamName = name,
                        TeamLeadId = leadId
                    };
                    await api.UpdateTeamAsync(_existingTeam.IdTeam, dto);
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
