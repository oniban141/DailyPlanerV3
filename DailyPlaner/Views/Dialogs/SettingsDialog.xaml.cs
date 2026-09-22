using System;
using System.Windows;

namespace DailyPlaner.Views.Dialogs
{
    public partial class SettingsDialog : Window
    {
        public bool IsDarkThemeRequested { get; private set; }
        public bool IsAutoStartRequested { get; private set; }

        public SettingsDialog(bool isDarkTheme, bool isAutoStart)
        {
            InitializeComponent();
            IsDarkThemeRequested = isDarkTheme;
            IsAutoStartRequested = isAutoStart;
            DarkThemeCheckBox.IsChecked = isDarkTheme;
            AutoStartCheckBox.IsChecked = isAutoStart;
            DarkThemeCheckBox.Focus();
        }

        private void DarkThemeCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            IsDarkThemeRequested = DarkThemeCheckBox.IsChecked == true;
            App.ApplyTheme(IsDarkThemeRequested);
        }

        private void AutoStartCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            IsAutoStartRequested = AutoStartCheckBox.IsChecked == true;
            App.SetAutoStart(IsAutoStartRequested);
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
