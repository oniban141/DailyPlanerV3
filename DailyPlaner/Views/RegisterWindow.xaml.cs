using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using DailyPlaner.ViewModels;

namespace DailyPlaner.Views
{
    public partial class RegisterWindow : Window
    {
        private RegisterViewModel ViewModel => (RegisterViewModel)DataContext;

        public RegisterWindow()
        {
            InitializeComponent();
            DataContext = new RegisterViewModel(new Services.DatabaseService());
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (ViewModel != null)
            {
                ViewModel.Password = ((PasswordBox)sender).Password;
            }
        }

        private void ConfirmPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (ViewModel != null)
            {
                ViewModel.ConfirmPassword = ((PasswordBox)sender).Password;
            }
        }
    }
}
