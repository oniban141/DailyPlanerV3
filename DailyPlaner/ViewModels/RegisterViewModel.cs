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
    public class RegisterViewModel : INotifyPropertyChanged
    {
        private readonly DatabaseService _databaseService;
        private string _username;
        private string _password;
        private string _confirmPassword;
        private string _email;
        private string _firstName;
        private string _lastName;
        private int _selectedGenderId;
        private List<Gender> _genders;

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

        public string ConfirmPassword
        {
            get => _confirmPassword;
            set
            {
                _confirmPassword = value;
                OnPropertyChanged(nameof(ConfirmPassword));
            }
        }

        public string Email
        {
            get => _email;
            set
            {
                _email = value;
                OnPropertyChanged(nameof(Email));
            }
        }

        public string FirstName
        {
            get => _firstName;
            set
            {
                _firstName = value;
                OnPropertyChanged(nameof(FirstName));
            }
        }

        public string LastName
        {
            get => _lastName;
            set
            {
                _lastName = value;
                OnPropertyChanged(nameof(LastName));
            }
        }

        public int SelectedGenderId
        {
            get => _selectedGenderId;
            set
            {
                _selectedGenderId = value;
                OnPropertyChanged(nameof(SelectedGenderId));
            }
        }

        public List<Gender> Genders
        {
            get => _genders;
            set
            {
                _genders = value;
                OnPropertyChanged(nameof(Genders));
            }
        }

        public ICommand RegisterCommand { get; }
        public ICommand CancelCommand { get; }

        public RegisterViewModel(DatabaseService databaseService)
        {
            _databaseService = databaseService;
            RegisterCommand = new RelayCommand(ExecuteRegister, CanExecuteRegister);
            CancelCommand = new RelayCommand(ExecuteCancel);
            LoadGenders();
        }

        private void LoadGenders()
        {
            try
            {
                Genders = _databaseService.GetAllGenders();
                if (Genders.Count > 0)
                {
                    SelectedGenderId = Genders[0].Id;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки списка полов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanExecuteRegister(object parameter)
        {
            return !string.IsNullOrWhiteSpace(Username) &&
                   !string.IsNullOrWhiteSpace(Password) &&
                   Password == ConfirmPassword &&
                   !string.IsNullOrWhiteSpace(Email) &&
                   !string.IsNullOrWhiteSpace(FirstName);
        }

        private void ExecuteRegister(object parameter)
        {
            try
            {
                var user = new User
                {
                    Username = Username,
                    Password = Password,
                    Email = Email,
                    FirstName = FirstName,
                    LastName = LastName,
                    GenderId = SelectedGenderId,
                    RoleId = 1,
                    CreatedDate = DateTime.Now
                };

                bool result = _databaseService.CreateUser(user);
                if (result)
                {
                    MessageBox.Show("Регистрация прошла успешно!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    CloseRegisterWindow();
                }
                else
                {
                    MessageBox.Show("Не удалось зарегистрироваться.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при регистрации: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteCancel(object parameter)
        {
            try
            {
                var window = parameter as Window;
                if (window != null)
                {
                    window.Close();
                }
                else
                {
                    CloseRegisterWindow();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при закрытии окна: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CloseRegisterWindow()
        {
            var registerWindow = Application.Current.Windows.OfType<Views.RegisterWindow>().FirstOrDefault();
            registerWindow?.Close();
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
