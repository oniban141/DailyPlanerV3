using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using DailyPlaner.ViewModels;

namespace DailyPlaner.Views
{
    public partial class RegisterWindow : Window
    {
        private RegisterViewModel ViewModel
        {
            get { return (RegisterViewModel)DataContext; }
        }

        public RegisterWindow()
        {
            InitializeComponent();
            App.SetWindowIcon(this);
            DataContext = new RegisterViewModel(new Services.DatabaseService());
            GenderComboBox.SelectionChanged += GenderComboBox_SelectionChanged;
            LoadHeaderIcon();
        }

        private void LoadHeaderIcon()
        {
            try
            {
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

        private void GenderComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (GenderWatermark != null)
            {
                GenderWatermark.Visibility = GenderComboBox.SelectedItem != null
                    ? Visibility.Collapsed
                    : Visibility.Visible;
            }
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            var passwordBox = (PasswordBox)sender;
            if (ViewModel != null)
            {
                ViewModel.Password = passwordBox.Password;
            }
            if (PasswordWatermark != null)
            {
                PasswordWatermark.Visibility = string.IsNullOrEmpty(passwordBox.Password)
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            }
        }

        private void ConfirmPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            var passwordBox = (PasswordBox)sender;
            if (ViewModel != null)
            {
                ViewModel.ConfirmPassword = passwordBox.Password;
            }
            if (ConfirmPasswordWatermark != null)
            {
                ConfirmPasswordWatermark.Visibility = string.IsNullOrEmpty(passwordBox.Password)
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            }
        }

        private void TextBox_TextChanged()
        {

        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}
