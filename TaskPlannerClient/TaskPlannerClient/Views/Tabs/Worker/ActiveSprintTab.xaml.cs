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

namespace TaskPlannerClient.Views.Tabs.Worker
{
    /// <summary>
    /// Логика взаимодействия для ActiveSprintTab.xaml
    /// </summary>
    public partial class ActiveSprintTab : UserControl
    {
        public ActiveSprintTab()
        {
            InitializeComponent();
            Loaded += async (s, e) => await LoadData();
        }

        private async System.Threading.Tasks.Task LoadData()
        {
            try
            {
                var sprint = await UserSession.Instance.Api.GetActiveSprintAsync();
                SprintNameText.Text = sprint.SprintName;
                StartDateText.Text = sprint.StartDate.ToString("yyyy-MM-dd");
                EndDateText.Text = sprint.EndDate.ToString("yyyy-MM-dd");
                WorkDaysText.Text = sprint.WorkDays.ToString();
            }
            catch (Exception ex)
            {
                SprintNameText.Text = $"Ошибка: {ex.Message}";
                StartDateText.Text = "—";
                EndDateText.Text = "—";
                WorkDaysText.Text = "—";
            }
        }
    }
}
