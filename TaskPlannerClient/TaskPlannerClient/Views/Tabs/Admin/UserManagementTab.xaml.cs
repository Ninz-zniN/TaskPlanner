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
    /// Логика взаимодействия для UserManagementTab.xaml
    /// </summary>
    public partial class UserManagementTab : UserControl
    {
        public ObservableCollection<UserAccount> Users { get; set; } = new ObservableCollection<UserAccount>();

        public UserManagementTab()
        {
            InitializeComponent();
            UsersGrid.ItemsSource = Users;
            Loaded += async (s, e) => await LoadUsers();
        }

        private async System.Threading.Tasks.Task LoadUsers()
        {
            try
            {
                var users = await UserSession.Instance.Api.GetUsersAsync();
                Users.Clear();
                foreach (var u in users)
                    Users.Add(u);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки пользователей: {ex.Message}");
            }
        }

        private async void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var window = new UserEditWindow();
            window.Owner = Window.GetWindow(this);
            if (window.ShowDialog() == true)
                await LoadUsers();
        }

        private async void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is UserAccount user)
            {
                var window = new UserEditWindow(user);
                window.Owner = Window.GetWindow(this);
                if (window.ShowDialog() == true)
                    await LoadUsers();
            }
        }

        private async void BlockButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is UserAccount user)
            {
                var result = MessageBox.Show($"Заблокировать пользователя '{user.Login}'?", "Подтверждение",
                    MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        await UserSession.Instance.Api.DeactivateUserAsync(user.IdUser);
                        await LoadUsers();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка блокировки: {ex.Message}");
                    }
                }
            }
        }

        private async void UnblockButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is UserAccount user)
            {
                try
                {
                    await UserSession.Instance.Api.UpdateUserAsync(user.IdUser, new UserUpdateDto { IsActive = true });
                    await LoadUsers();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка разблокировки: {ex.Message}");
                }
            }
        }

        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            await LoadUsers();
        }
    }
}
