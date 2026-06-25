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
    /// Логика взаимодействия для UserEditWindow.xaml
    /// </summary>
    public partial class UserEditWindow : Window
    {
        private readonly UserAccount? _existingUser;
        private List<Employee> _employees;

        public UserEditWindow(UserAccount? user = null)
        {
            InitializeComponent();
            _existingUser = user;
            Loaded += OnLoaded;
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var api = UserSession.Instance.Api;
                _employees = await DataCache.GetEmployeesAsync(); //await api.GetEmployeesAsync();

                var empItems = new List<Employee> { new Employee { IdEmployee = -1, LastName = "Не привязан", FirstName = "", Position = "" } };
                empItems.AddRange(_employees);
                EmployeeComboBox.ItemsSource = empItems;

                // Роль по умолчанию
                RoleComboBox.SelectedValue = "worker";

                if (_existingUser != null)
                {
                    LoginTextBox.Text = _existingUser.Login;
                    PasswordTextBox.Text = ""; // пароль не отображаем
                    RoleComboBox.SelectedValue = _existingUser.Role;
                    if (_existingUser.employee != null)
                        EmployeeComboBox.SelectedValue = _existingUser.employee.IdEmployee;
                    else
                        EmployeeComboBox.SelectedValue = -1;
                }
                else
                {
                    EmployeeComboBox.SelectedValue = -1;
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
                string login = LoginTextBox.Text.Trim();
                string password = PasswordTextBox.Text.Trim();
                string role = RoleComboBox.SelectedValue as string ?? "worker";

                if (string.IsNullOrWhiteSpace(login))
                {
                    MessageBox.Show("Логин обязателен.");
                    return;
                }

                int? employeeId = EmployeeComboBox.SelectedValue as int?;
                if (employeeId == -1) employeeId = null;

                var api = UserSession.Instance.Api;
                if (_existingUser == null)
                {
                    if (string.IsNullOrWhiteSpace(password))
                    {
                        MessageBox.Show("Пароль обязателен для нового пользователя.");
                        return;
                    }
                    var dto = new CreateUserDto(login, password, role, employeeId);
                    await api.CreateUserAsync(dto);
                }
                else
                {
                    var dto = new UserUpdateDto
                    {
                        Login = login,
                        Role = role,
                        EmployeeId = employeeId
                    };
                    if (!string.IsNullOrWhiteSpace(password))
                        dto.Password = password;

                    await api.UpdateUserAsync(_existingUser.IdUser, dto);
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
