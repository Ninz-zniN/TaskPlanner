using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using TaskPlannerClient.Converters;
using TaskPlannerClient.Models;
using TaskPlannerClient.Service;
using TaskPlannerClient.Views.Tabs.Admin;
using TaskPlannerClient.Views.Tabs.Common;
using TaskPlannerClient.Views.Tabs.Manager;
using TaskPlannerClient.Views.Tabs.Worker;

namespace TaskPlannerClient.Views
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            var user = UserSession.Instance.CurrentUser;
            var converter = new RoleToDisplayConverter();
            string roleDisplay = (string)converter.Convert(user.Role, typeof(string), null, CultureInfo.CurrentCulture);
            UserInfoText.Text = $"{user.Login} ({roleDisplay})";
            if (user.Employee != null)
                UserInfoText.Text += $" - {user.Employee.LastName} {user.Employee.FirstName}";

            // Показываем кнопку «Отчёты» только для manager и admin
            ReportsButton.Visibility = (user.Role == "project_manager" || user.Role == "admin") ? Visibility.Visible : Visibility.Collapsed;

            Loaded += MainWindow_Loaded;
        }

        private void ReportsButton_Click(object sender, RoutedEventArgs e)
        {
            var reportWindow = new ReportWindow();
            reportWindow.Owner = this;
            reportWindow.ShowDialog();
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            BuildTabsBasedOnRole();
        }

        private void BuildTabsBasedOnRole()
        {
            MainTabControl.Items.Clear();
            var role = UserSession.Instance.CurrentUser.Role;
            var api = UserSession.Instance.Api;

            if (role == "worker")
            {
                // worker использует свои вкладки; справочники можно загружать внутри вкладок
                MainTabControl.Items.Add(new TabItem { Header = "Мои задачи", Content = new TasksContainerTab() });
                MainTabControl.Items.Add(new TabItem { Header = "Все задачи", Content = new AllTaskTab() });
                MainTabControl.Items.Add(new TabItem { Header = "Моя нагрузка", Content = new MyLoadTab() });
                MainTabControl.Items.Add(new TabItem { Header = "Активный спринт", Content = new ActiveSprintTab() });
                MainTabControl.Items.Add(new TabItem { Header = "Справочники", Content = new ReferencesTab() });
            }
            else if (role == "project_manager" || role == "admin")
            {
                MainTabControl.Items.Add(new TabItem { Header = "Задачи", Content = new TasksManagementTab() });
                MainTabControl.Items.Add(new TabItem { Header = "Проекты", Content = new ProjectsTab() });
                MainTabControl.Items.Add(new TabItem { Header = "Спринты", Content = new SprintsTab() });

                if (role == "admin")
                {
                    MainTabControl.Items.Add(new TabItem { Header = "Управление персоналом", Content = new PersonnelManagementTab() });
                    MainTabControl.Items.Add(new TabItem { Header = "Управление командами", Content = new TeamManagementTab() });
                }
                else
                    MainTabControl.Items.Add(new TabItem { Header = "Сотрудники", Content = new EmployeesViewTab() });
            }
        }

        public void OpenEisenhowerMatrix(int? statusId)
        {
            var matrix = new Views.Tabs.Common.EisenhowerMatrixTab();
            var window = new Window
            {
                Title = "Матрица Эйзенхауэра",
                Content = matrix,
                Width = 1000,
                Height = 700,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this
            };
            window.Loaded += (s, e) =>
            {
                // После загрузки окна применяем фильтр и загружаем данные
                matrix.ApplyExternalStatusFilter(statusId);
                // Загрузка данных запустится внутри ApplyExternalStatusFilter
            };
            window.ShowDialog();
        }
        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            // Сброс сессии (опционально)
            // UserSession.Instance.Reset();
            var loginWindow = new LoginWindow();
            loginWindow.Show();
            this.Close();
        }
    }
}
