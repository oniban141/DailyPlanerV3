using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using DailyPlaner.Models;
using DailyPlaner.Services;

namespace DailyPlaner.ViewModels
{
    public class LoginViewModel : INotifyPropertyChanged
    {
        private readonly DatabaseService _databaseService;
        private readonly NotificationService _notificationService;
        private string _username;
        private string _password;
        private bool _isLoading;

        public event PropertyChangedEventHandler PropertyChanged;

        public string Username
        {
            get => _username;
            set
            {
                _username = value;
                OnPropertyChanged(nameof(Username));
            }
        }

        public string Password
        {
            get => _password;
            set
            {
                _password = value;
                OnPropertyChanged(nameof(Password));
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged(nameof(IsLoading));
            }
        }

        public ICommand LoginCommand { get; }
        public ICommand RegisterCommand { get; }

        public LoginViewModel()
        {
            _databaseService = new DatabaseService();
            _notificationService = new NotificationService();
            LoginCommand = new RelayCommand(ExecuteLogin, CanExecuteLogin);
            RegisterCommand = new RelayCommand(ExecuteRegister);
        }

        private bool CanExecuteLogin(object parameter)
        {
            return !string.IsNullOrWhiteSpace(Username) && !string.IsNullOrWhiteSpace(Password);
        }

        private void ExecuteLogin(object parameter)
        {
            IsLoading = true;
            try
            {
                var user = _databaseService.GetUserByUsername(Username);
                if (user != null && user.PasswordHash == DatabaseService.HashPassword(Password))
                {
                    var mainWindow = new MainWindow(user);
                    mainWindow.Show();

                    var loginHost = Application.Current.MainWindow;
                    Application.Current.MainWindow = mainWindow;
                    loginHost?.Close();
                }
                else
                {
                    MessageBox.Show("Неверное имя пользователя или пароль.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(ex.Message, "Ошибка базы данных", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при входе: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void ExecuteRegister(object parameter)
        {
            try
            {
                var registerWindow = new Views.RegisterWindow();
                var registerViewModel = new RegisterViewModel(_databaseService);
                registerWindow.DataContext = registerViewModel;
                App.SetWindowIcon(registerWindow);
                registerWindow.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при открытии регистрации: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
