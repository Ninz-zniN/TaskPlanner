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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TaskPlannerClient.Views.Tabs.Admin
{
    /// <summary>
    /// Логика взаимодействия для PersonnelManagementTab.xaml
    /// </summary>
    public partial class PersonnelManagementTab : UserControl
    {
        public PersonnelManagementTab()
        {
            InitializeComponent();

            // Добавляем подвкладки
            InnerTabControl.Items.Add(new TabItem
            {
                Header = "Сотрудники",
                Content = new EmployeeManagementTab()
            });

            InnerTabControl.Items.Add(new TabItem
            {
                Header = "Пользователи",
                Content = new UserManagementTab()
            });
        }
    }
}
