using System;
using System.Windows;

namespace DailyPlaner.Views.Dialogs
{
    public partial class EventDialog : Window
    {
        public string EventTitle => TitleBox.Text.Trim();
        public string EventDescription => DescriptionBox.Text.Trim();
        public string EventLocation => LocationBox.Text.Trim();
        public DateTime EventStart => StartDatePicker.SelectedDate ?? DateTime.Today;

        public EventDialog(string title, string description, string location, DateTime start)
        {
            InitializeComponent();
            TitleBox.Text = title ?? string.Empty;
            DescriptionBox.Text = description ?? string.Empty;
            LocationBox.Text = location ?? string.Empty;
            StartDatePicker.SelectedDate = start;
            Loaded += (s, e) => TitleBox.Focus();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(EventTitle))
            {
                MessageBox.Show("Введите название события.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void TitleBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {

        }
    }
}
