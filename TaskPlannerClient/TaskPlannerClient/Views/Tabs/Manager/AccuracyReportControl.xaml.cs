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
using TaskPlannerClient.Service;

namespace TaskPlannerClient.Views.Tabs.Manager
{
    /// <summary>
    /// Логика взаимодействия для AccuracyReportControl.xaml
    /// </summary>
    public partial class AccuracyReportControl : UserControl
    {
        public ObservableCollection<AccuracyItem> AccuracyItems { get; set; } = new ObservableCollection<AccuracyItem>();

        /// <summary>
        /// Возвращает сводный текст (название спринта и общие показатели) для экспорта.
        /// </summary>
        public string SummaryTextContent => SummaryText.Text.TrimEnd('\n', '\r');

        /// <summary>
        /// Возвращает текст с общим отклонением для экспорта.
        /// </summary>
        public string TotalTextContent => TotalText.Text.TrimEnd('\n', '\r');

        public AccuracyReportControl()
        {
            InitializeComponent();
            AccuracyDataGrid.ItemsSource = AccuracyItems;
            Loaded += OnLoaded;
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var api = UserSession.Instance.Api;
                var sprints = await DataCache.GetSprintsAsync(); //await api.GetSprintsAsync();
                SprintFilterCombo.ItemsSource = sprints;
                if (sprints.Any())
                    SprintFilterCombo.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки спринтов: {ex.Message}");
            }
        }

        private async void SprintFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SprintFilterCombo.SelectedValue is int sprintId)
            {
                try
                {
                    var report = await UserSession.Instance.Api.GetAccuracyReportAsync(sprintId);
                    AccuracyItems.Clear();
                    foreach (var item in report.Items)
                        AccuracyItems.Add(item);

                    SummaryText.Text = $"Спринт: {((Sprint)SprintFilterCombo.SelectedItem).SprintName} | " +
                                      $"Всего оценка: {report.TotalEstimate} ч | Факт: {report.TotalActual} ч | " +
                                      $"Отклонение: {report.OverallDeviationPercent:F1}%";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка загрузки точности оценок: {ex.Message}");
                }
            }
        }

        private void OpenStatusHistoryButton_Click(object sender, RoutedEventArgs e)
        {
            var window = new StatusHistoryWindow();
            window.Owner = Window.GetWindow(this);
            window.ShowDialog();
        }
    }
}
