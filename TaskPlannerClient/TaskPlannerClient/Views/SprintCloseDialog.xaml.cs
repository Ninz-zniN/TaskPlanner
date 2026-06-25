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

namespace TaskPlannerClient.Views
{
    /// <summary>
    /// Логика взаимодействия для SprintCloseDialog.xaml
    /// </summary>
    public partial class SprintCloseDialog : Window
    {
        public string SelectedAction { get; private set; } = "cancel";
        public SprintCloseDialog()
        {
            InitializeComponent();
        }

        private void MoveToNextButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedAction = "move_to_next";
            this.DialogResult = true;
            this.Close();
        }

        private void MoveToBacklogButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedAction = "move_to_backlog";
            this.DialogResult = true;
            this.Close();
        }

        private void CancelTasksButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedAction = "cancel_tasks";
            this.DialogResult = true;
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedAction = "cancel";
            this.DialogResult = false;
            this.Close();
        }
    }
}
