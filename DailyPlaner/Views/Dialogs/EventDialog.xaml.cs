using System;
using System.Collections.Generic;
using System.Windows;

namespace DailyPlaner.Views.Dialogs
{
    public partial class EventDialog : Window
    {
        private static readonly int[] ReminderOffsetMinutes = { 5, 10, 15, 30, 60, 120, 1440 };

        public string EventTitle => TitleBox.Text.Trim();
        public string EventDescription => DescriptionBox.Text.Trim();
        public string EventLocation => LocationBox.Text.Trim();
        public DateTime EventStart { get; private set; }
        public bool ReminderEnabled => ReminderCheckBox.IsChecked == true;
        public DateTime? ReminderTime { get; private set; }

        public EventDialog(string title, string description, string location, DateTime start)
        {
            InitializeComponent();

            FillTimeItems(start);
            StartDatePicker.SelectedDate = start.Date;
            SelectTime(start.TimeOfDay);

            ReminderOffsetComboBox.ItemsSource = new[]
            {
                "5 минут", "10 минут", "15 минут", "30 минут", "1 час", "2 часа", "За день"
            };
            ReminderOffsetComboBox.SelectedIndex = 3;

            TitleBox.Text = title ?? string.Empty;
            DescriptionBox.Text = description ?? string.Empty;
            LocationBox.Text = location ?? string.Empty;
            Loaded += (s, e) => TitleBox.Focus();
        }

        private void FillTimeItems(DateTime baseTime)
        {
            var items = new List<string>();
            var start = baseTime.Date;
            for (int i = 0; i < 48; i++)
            {
                items.Add(start.ToString("HH:mm"));
                start = start.AddMinutes(30);
            }
            TimeComboBox.ItemsSource = items;
        }

        private void SelectTime(TimeSpan time)
        {
            var rounded = new TimeSpan(time.Hours, time.Minutes >= 30 ? 30 : 0, 0);
            string target = ((int)rounded.TotalHours).ToString("00") + ":" + rounded.Minutes.ToString("00");
            foreach (var item in TimeComboBox.Items)
            {
                if (item.ToString() == target)
                {
                    TimeComboBox.SelectedItem = item;
                    return;
                }
            }
            if (TimeComboBox.Items.Count > 0)
            {
                TimeComboBox.SelectedIndex = 0;
            }
        }

        private void ReminderCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            ReminderPanel.Visibility = ReminderCheckBox.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
        }

        private void ReminderOffsetComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (ReminderCheckBox.IsChecked == true)
            {
                UpdateReminderTime();
            }
        }

        private void UpdateReminderTime()
        {
            int index = ReminderOffsetComboBox.SelectedIndex;
            if (index < 0 || index >= ReminderOffsetMinutes.Length)
            {
                ReminderTime = null;
                return;
            }
            ReminderTime = EventStart.AddMinutes(-ReminderOffsetMinutes[index]);
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(EventTitle))
            {
                MessageBox.Show("Введите название события.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var date = StartDatePicker.SelectedDate ?? DateTime.Today;
            var time = TimeSpan.Zero;
            if (TimeComboBox.SelectedItem != null && TimeSpan.TryParse(TimeComboBox.SelectedItem.ToString(), out var parsed))
            {
                time = parsed;
            }
            EventStart = date.Date + time;

            if (ReminderEnabled)
            {
                UpdateReminderTime();
            }
            else
            {
                ReminderTime = null;
            }

            DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
