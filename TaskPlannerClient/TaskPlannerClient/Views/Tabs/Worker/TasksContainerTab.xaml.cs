using LiveChartsCore;
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
using TaskPlannerClient.Service;
using TaskPlannerClient.Views.Tabs.Common;

namespace TaskPlannerClient.Views.Tabs.Worker
{
    /// <summary>
    /// Логика взаимодействия для TasksContainerTab.xaml
    /// </summary>
    public partial class TasksContainerTab : UserControl
    {
        public TasksContainerTab()
        {
            InitializeComponent();

            // Канбан (основной вид по умолчанию)
            var kanban = new KanbanTab();
            kanban.SetAssigneeFilter(UserSession.Instance.CurrentUser.Employee?.IdEmployee);
            InnerTabControl.Items.Add(new TabItem { Header = "Канбан", Content = kanban });

            // Список (бывший MyTasksTab)
            InnerTabControl.Items.Add(new TabItem { Header = "Список", Content = new MyTasksTab() });

            // Матрица
            InnerTabControl.Items.Add(new TabItem { Header = "Матрица", Content = new EisenhowerMatrixTab() });

            // Календарь
            InnerTabControl.Items.Add(new TabItem { Header = "Календарь", Content = new DeadlineCalendarTab() });
        }
    }
}
