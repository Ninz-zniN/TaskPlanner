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
using TaskPlannerClient.Models;
using TaskPlannerClient.Service;

namespace TaskPlannerClient.Views.Tabs.Worker
{
    /// <summary>
    /// Логика взаимодействия для ReferencesTab.xaml
    /// </summary>
    public partial class ReferencesTab : UserControl
    {
        public ReferencesTab()
        {
            InitializeComponent();
            Loaded += async (s, e) => await LoadData();
        }

        private async System.Threading.Tasks.Task LoadData()
        {
            var api = UserSession.Instance.Api;
            TaskTypesGrid.ItemsSource = DataCache.TaskTypes; //ReferenceCache.TaskTypes; //await api.GetTaskTypesAsync();
            TaskStatusesGrid.ItemsSource = DataCache.TaskStatuses; //ReferenceCache.TaskStatuses; await api.GetTaskStatusesAsync();
            PrioritiesGrid.ItemsSource = DataCache.Priorities; //ReferenceCache.Priorities;//await api.GetPrioritiesAsync();
        }
    }
}
