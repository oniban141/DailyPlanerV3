using System;
using System.Windows;

namespace DailyPlaner.Views.Dialogs
{
    public partial class TaskDialog : Window
    {
        public string TaskTitle => TitleBox.Text.Trim();
        public string TaskDescription => DescriptionBox.Text.Trim();
        public DateTime TaskDueDate => DueDatePicker.SelectedDate ?? DateTime.Today;

        public TaskDialog(string title, string description, DateTime dueDate)
        {
            InitializeComponent();
            TitleBox.Text = title ?? string.Empty;
            DescriptionBox.Text = description ?? string.Empty;
            DueDatePicker.SelectedDate = dueDate;
            Loaded += (s, e) => TitleBox.Focus();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TaskTitle))
            {
                MessageBox.Show("Введите название задачи.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
