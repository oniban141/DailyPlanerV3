using System;
using System.Windows;

namespace DailyPlaner.Views.Dialogs
{
    public partial class SettingsDialog : Window
    {
        public bool IsDarkThemeRequested { get; private set; }

        public SettingsDialog(bool isDarkTheme)
        {
            InitializeComponent();
            DarkThemeCheckBox.IsChecked = isDarkTheme;
            DarkThemeCheckBox.Focus();
        }

        private void DarkThemeCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            IsDarkThemeRequested = DarkThemeCheckBox.IsChecked == true;
            App.ApplyTheme(IsDarkThemeRequested);
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
