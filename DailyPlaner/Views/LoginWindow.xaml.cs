using System.Windows;
using System.Windows.Controls;
using DailyPlaner.ViewModels;

namespace DailyPlaner.Views
{
    public partial class LoginWindow : Page
    {
        public LoginWindow()
        {
            InitializeComponent();
            DataContext = new LoginViewModel();
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
