using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using DailyPlaner.ViewModels;

namespace DailyPlaner.Views
{
    public partial class LoginWindow : Page
    {
        public LoginWindow()
        {
            InitializeComponent();
            DataContext = new LoginViewModel();
            Loaded += (s, e) => LoadHeaderIcon();
        }

        private void LoadHeaderIcon()
        {
            try
            {
                if (HeaderIcon.Source != null)
                {
                    return;
                }
                string iconPath = App.FindIconFile();
                if (iconPath != null)
                {
                    var bitmap = new BitmapImage(new Uri(iconPath));
                    bitmap.Freeze();
                    HeaderIcon.Source = bitmap;
                }
            }
            catch (Exception)
            {
            }
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            var passwordBox = (PasswordBox)sender;
            if (DataContext is LoginViewModel viewModel)
            {
                viewModel.Password = passwordBox.Password;
            }
            if (PasswordWatermark != null)
            {
                PasswordWatermark.Visibility = string.IsNullOrEmpty(passwordBox.Password)
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            }
        }
    }
}
