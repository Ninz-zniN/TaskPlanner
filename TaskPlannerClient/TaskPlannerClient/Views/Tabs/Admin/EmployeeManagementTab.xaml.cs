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
using TaskPlannerClient.Models.Dto;
using TaskPlannerClient.Service;

namespace TaskPlannerClient.Views.Tabs.Admin
{
    /// <summary>
    /// Логика взаимодействия для EmployeeManagementTab.xaml
    /// </summary>
    public partial class EmployeeManagementTab : UserControl
    {
        public ObservableCollection<Employee> Employees { get; set; } = new ObservableCollection<Employee>();

        public EmployeeManagementTab()
        {
            InitializeComponent();
            EmployeesGrid.ItemsSource = Employees;
            Loaded += async (s, e) => await LoadEmployees();
        }

        private async System.Threading.Tasks.Task LoadEmployees()
        {
            try
            {
                await DataCache.GetEmployeesAsync(true);
                var employees = await UserSession.Instance.Api.GetEmployeesAsync(true);
                Employees.Clear();
                foreach (var emp in employees)
                    Employees.Add(emp);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки сотрудников: {ex.Message}");
            }
        }

        private async void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var window = new EmployeeEditWindow();
            window.Owner = Window.GetWindow(this);
            if (window.ShowDialog() == true)
                await LoadEmployees();
        }

        private async void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is Employee employee)
            {
                var window = new EmployeeEditWindow(employee);
                window.Owner = Window.GetWindow(this);
                if (window.ShowDialog() == true)
                    await LoadEmployees();
            }
        }

        private async void DismissButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is Employee employee)
            {
                var result = MessageBox.Show($"Уволить сотрудника {employee.LastName} {employee.FirstName}?", "Подтверждение",
                    MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        await UserSession.Instance.Api.DismissEmployeeAsync(employee.IdEmployee);
                        await LoadEmployees();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка увольнения: {ex.Message}");
                    }
                }
            }
        }

        private async void RestoreButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is Employee employee)
            {
                try
                {
                    await UserSession.Instance.Api.UpdateEmployeeAsync(employee.IdEmployee, new EmployeeUpdateDto { IsDismissed = false });
                    await LoadEmployees();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка восстановления: {ex.Message}");
                }
            }
        }

        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            await LoadEmployees();
        }
    }
}
