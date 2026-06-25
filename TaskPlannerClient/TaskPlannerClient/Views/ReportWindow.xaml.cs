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
using TaskPlannerClient.Views.Tabs.Manager;

namespace TaskPlannerClient.Views
{
    /// <summary>
    /// Логика взаимодействия для ReportWindow.xaml
    /// </summary>
    public partial class ReportWindow : Window
    {
        private static readonly Dictionary<string, MyColumnDefinition> LoadColumns = new()
        {
            { "Employee.LastName + \" \" + Employee.FirstName", new() { Header = "Сотрудник", Weight = 3 } },
            { "CurrentLoadHours", new() { Header = "Текущие часы", Weight = 1 } },
            { "CapacityHours", new() { Header = "Ёмкость (часы)", Weight = 1 } },
            { "LoadPercent", new() { Header = "Загрузка %", Weight = 1 } }
        };

        private static readonly Dictionary<string, MyColumnDefinition> OverdueColumns = new()
        {
            { "Title", new() { Header = "Задача", Weight = 3 } },
            { "Deadline", new() { Header = "Дедлайн", Weight = 1.5 } },
            { "Assignee.LastName", new() { Header = "Исполнитель", Weight = 2 } },
            { "Project.Name", new() { Header = "Проект", Weight = 2 } }
        };

        private static readonly Dictionary<string, MyColumnDefinition> AccuracyColumns = new()
        {
            { "Task.Title", new() { Header = "Задача", Weight = 3 } },
            { "EstimateHours", new() { Header = "Оценка (ч)", Weight = 1 } },
            { "ActualHours", new() { Header = "Факт (ч)", Weight = 1 } },
            { "Deviation", new() { Header = "Отклонение (ч)", Weight = 1 } },
            { "DeviationPercent", new() { Header = "Отклонение %", Weight = 1 } }
        };

        private static readonly Dictionary<string, MyColumnDefinition> ProgressColumns = new()
        {
            { "ProjectName", new() { Header = "Проект", Weight = 3 } },
            { "TotalTasks", new() { Header = "Всего задач", Weight = 1 } },
            { "CompletedTasks", new() { Header = "Выполнено", Weight = 1 } },
            { "PlannedHours", new() { Header = "План (ч)", Weight = 1.2 } },
            { "ActualHours", new() { Header = "Факт (ч)", Weight = 1.2 } },
            { "CompletionPercentWeighted", new() { Header = "Освоено %", Weight = 1.2 } }
        };

        private static readonly Dictionary<string, MyColumnDefinition> SummaryColumns = new()
        {
            { "Title", new() { Header = "Задача", Weight = 3 } },
            { "EstimateHours", new() { Header = "Оценка (ч)", Weight = 1 } },
            { "ActualHours", new() { Header = "Факт (ч)", Weight = 1 } },
            { "WorkHours", new() { Header = "В работе (ч)", Weight = 1 } },
            { "ReviewHours", new() { Header = "Ревью (ч)", Weight = 1 } },
            { "TestHours", new() { Header = "Тест (ч)", Weight = 1 } }
        };
        public ReportWindow()
        {
            InitializeComponent();
            LoadReports();
        }

        private void LoadReports()
        {

            ReportsTabControl.Items.Add(new TabItem { Header = "Нагрузка", Content = new LoadReportControl() });
            ReportsTabControl.Items.Add(new TabItem { Header = "Просроченные задачи", Content = new OverdueReportControl() });
            ReportsTabControl.Items.Add(new TabItem { Header = "Точность оценок", Content = new AccuracyReportControl() });
            ReportsTabControl.Items.Add(new TabItem { Header = "Прогресс проектов", Content = new ProjectProgressReportControl() });
            ReportsTabControl.Items.Add(new TabItem { Header = "Сводка проекта", Content = new ProjectSummaryReportControl() });

        }

        private void OpenBurndownButton_Click(object sender, RoutedEventArgs e)
        {
            var burndownWindow = new BurndownReportWindow();
            burndownWindow.Owner = this;
            burndownWindow.ShowDialog();
        }

        private void ExportPdfButton_Click(object sender, RoutedEventArgs e)
        {
            if (ReportsTabControl.SelectedContent is not UserControl control) return;

            string? summary = null;

            if (control is LoadReportControl load)
            {
                summary = $"Команда: {load.SelectedTeamName}";
                ExportService.ExportToPdf(load.LoadItems, LoadColumns, "Нагрузка команды", headerSummary: summary);
            }
            else if (control is OverdueReportControl overdue)
            {
                ExportService.ExportToPdf(overdue.OverdueItems, OverdueColumns, "Просроченные задачи");
            }
            else if (control is AccuracyReportControl accuracy)
            {
                summary = accuracy.SummaryTextContent + "\n" + accuracy.TotalTextContent;
                ExportService.ExportToPdf(accuracy.AccuracyItems, AccuracyColumns, "Точность оценок", headerSummary: summary);
            }
            else if (control is ProjectProgressReportControl progress)
            {
                ExportService.ExportToPdf(progress.Items, ProgressColumns, "Прогресс проектов");
            }
            else if (control is ProjectSummaryReportControl projectSummary)
            {
                summary = projectSummary.ProjectSummaryText + "\n" + projectSummary.ProjectTotalText;
                ExportService.ExportToPdf(projectSummary.Tasks, SummaryColumns, "Сводка проекта", headerSummary: summary);
            }
        }

        private void ExportExcelButton_Click(object sender, RoutedEventArgs e)
        {
            if (ReportsTabControl.SelectedContent is not UserControl control) return;

            string? summary = null;

            if (control is LoadReportControl load)
            {
                summary = $"Команда: {load.SelectedTeamName}";
                ExportService.ExportToExcel(load.LoadItems, LoadColumns, "Нагрузка команды", headerSummary: summary);
            }
            else if (control is OverdueReportControl overdue)
            {
                ExportService.ExportToExcel(overdue.OverdueItems, OverdueColumns, "Просроченные задачи");
            }
            else if (control is AccuracyReportControl accuracy)
            {
                summary = accuracy.SummaryTextContent + "\n" + accuracy.TotalTextContent;
                ExportService.ExportToExcel(accuracy.AccuracyItems, AccuracyColumns, "Точность оценок", headerSummary: summary);
            }
            else if (control is ProjectProgressReportControl progress)
            {
                ExportService.ExportToExcel(progress.Items, ProgressColumns, "Прогресс проектов");
            }
            else if (control is ProjectSummaryReportControl projectSummary)
            {
                summary = projectSummary.ProjectSummaryText + "\n" + projectSummary.ProjectTotalText;
                ExportService.ExportToExcel(projectSummary.Tasks, SummaryColumns, "Сводка проекта", headerSummary: summary);
            }
        }

    }
}
