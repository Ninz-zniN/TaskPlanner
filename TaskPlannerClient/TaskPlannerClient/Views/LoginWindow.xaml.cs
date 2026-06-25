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
using TaskPlannerClient.Service;

namespace TaskPlannerClient.Views
{
    /// <summary>
    /// Логика взаимодействия для LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        private readonly ApiService _apiService;

        public LoginWindow()
        {
            InitializeComponent();
            _apiService = new ApiService();

            // Для удобства тестирования
            LoginTextBox.Text = "manager";
            PasswordBox.Password = "manager";
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ErrorText.Text = "";
                LoginButton.IsEnabled = false;

                var api = new ApiService(); // временный экземпляр только для логина
                var response = await api.LoginAsync(LoginTextBox.Text, PasswordBox.Password);

                // Инициализируем глобальную сессию
                UserSession.Instance.Initialize(response.User, response.Token);
                await DataCache.LoadReferencesAsync(); //ReferenceCache.LoadAllAsync();
                await LoadData();
                var mainWindow = new MainWindow();
                mainWindow.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                ErrorText.Text = "Ошибка входа: " + ex.Message;
            }
            finally
            {
                LoginButton.IsEnabled = true;
            }
        }
        //необязательный метод являющийся костылем чтобы делать загрузку основных данных в кэш до входа в главное окно
        private async Task LoadData()
        {
            await DataCache.GetEmployeesAsync();
            await DataCache.GetProjectsAsync();
            await DataCache.GetSprintsAsync();
            await DataCache.GetTeamsAsync();
        }
    }
}
