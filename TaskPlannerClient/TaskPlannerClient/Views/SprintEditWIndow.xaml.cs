using System;
using System.Collections;
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
using TaskPlannerClient.Models;
using TaskPlannerClient.Models.Dto;
using TaskPlannerClient.Service;

namespace TaskPlannerClient.Views
{
    /// <summary>
    /// Логика взаимодействия для SprintEditWIndow.xaml
    /// </summary>
    public partial class SprintEditWIndow : Window
    {
        private readonly Sprint? _existingSprint;

        public SprintEditWIndow(Sprint? sprint = null)
        {
            InitializeComponent();
            _existingSprint = sprint;
            LoadSprintData();
        }
        private void LoadSprintData()
        {
            if (_existingSprint != null)
            {
                SprintNameTextBox.Text = _existingSprint.SprintName;
                StartDatePicker.SelectedDate = _existingSprint.StartDate;
                EndDatePicker.SelectedDate = _existingSprint.EndDate;
                WorkDaysTextBox.Text = _existingSprint.WorkDays.ToString();
            }
            else
            {
                // Значения по умолчанию для нового спринта
                StartDatePicker.SelectedDate = DateTime.Today;
                EndDatePicker.SelectedDate = DateTime.Today.AddDays(13);
                WorkDaysTextBox.Text = "10";
            }
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveButton.IsEnabled = false;
                string name = SprintNameTextBox.Text.Trim();
                if (string.IsNullOrWhiteSpace(name) || !StartDatePicker.SelectedDate.HasValue || !EndDatePicker.SelectedDate.HasValue)
                {
                    MessageBox.Show("Все поля обязательны.");
                    return;
                }
                if (!int.TryParse(WorkDaysTextBox.Text, out int workDays) || workDays <= 0)
                {
                    MessageBox.Show("Некорректное число рабочих дней.");
                    return;
                }

                var start = StartDatePicker.SelectedDate.Value;
                var end = EndDatePicker.SelectedDate.Value;
                var api = UserSession.Instance.Api;
                if (_existingSprint == null)
                {
                    var dto = new CreateSprintDto(name, start, end, workDays);
                    await api.CreateSprintAsync(dto);
                }
                else
                {
                    var dto = new SprintUpdateDto
                    {
                        SprintName = name,
                        StartDate = start,
                        EndDate = end,
                        WorkDays = workDays
                    };
                    await api.UpdateSprintAsync(_existingSprint.IdSprint, dto);
                }

                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}");
            }
            finally
            {
                SaveButton.IsEnabled = true;
            }
        }
    }
}
