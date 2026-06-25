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
    /// Логика взаимодействия для MyLoadTab.xaml
    /// </summary>
    public partial class MyLoadTab : UserControl
    {
        public MyLoadTab()
        {
            InitializeComponent();
            Loaded += async (s, e) => await LoadData();
        }

        private async System.Threading.Tasks.Task LoadData()
        {
            try
            {
                var employeeId = UserSession.Instance.CurrentUser.Employee?.IdEmployee;
                if (employeeId == null)
                {
                    SprintInfoText.Text = "Нет привязки к сотруднику.";
                    return;
                }

                var report = await UserSession.Instance.Api.GetLoadReportAsync(employeeId);
                //SprintInfoText.Text = $"Спринт: {report.Sprint.Name} (рабочих дней: {report.Sprint.WorkDays})";

                if (report.Items.Count > 0)
                {
                    var load = report.Items[0];
                    CurrentLoadText.Text = $"{load.CurrentLoadHours} ч.";
                    CapacityText.Text = $"{load.CapacityHours} ч.";
                    LoadPercentText.Text = $"{load.LoadPercent:F1}%";
                }
                else
                {
                    SprintInfoText.Text += "\nНет данных о нагрузке.";
                }
            }
            catch (Exception ex)
            {
                SprintInfoText.Text = $"Ошибка загрузки: {ex.Message}";
            }
        }
    }
}
