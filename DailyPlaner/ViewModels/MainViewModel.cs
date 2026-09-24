using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using CsvHelper;
using Newtonsoft.Json;
using DailyPlaner.Models;
using DailyPlaner.Services;

namespace DailyPlaner.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly DatabaseService _databaseService;
        private readonly NotificationService _notificationService;
        private ReminderScheduler _reminderScheduler;
        private User _currentUser;
        private ObservableCollection<Models.Task> _tasks;
        private ObservableCollection<Event> _events;
        private ObservableCollection<Note> _notes;
        private ObservableCollection<Tag> _tags;
        private Models.Task _selectedTask;
        private Event _selectedEvent;
        private Note _selectedNote;
        private string _searchText;
        private Tag _selectedTag;
        private DateTime _selectedDate;
        private bool _isDarkTheme;

        public event PropertyChangedEventHandler PropertyChanged;

        public User CurrentUser
        {
            get => _currentUser;
            set
            {
                _currentUser = value;
                OnPropertyChanged(nameof(CurrentUser));
                if (_currentUser != null)
                {
                    LoadUserData();
                    StartReminderScheduler();
                }
            }
        }

        public ObservableCollection<Models.Task> Tasks
        {
            get => _tasks;
            set
            {
                _tasks = value;
                OnPropertyChanged(nameof(Tasks));
            }
        }

        public ObservableCollection<Event> Events
        {
            get => _events;
            set
            {
                _events = value;
                OnPropertyChanged(nameof(Events));
            }
        }

        public ObservableCollection<Note> Notes
        {
            get => _notes;
            set
            {
                _notes = value;
                OnPropertyChanged(nameof(Notes));
            }
        }

        public ObservableCollection<Tag> Tags
        {
            get => _tags;
            set
            {
                _tags = value;
                OnPropertyChanged(nameof(Tags));
            }
        }

        public Models.Task SelectedTask
        {
            get => _selectedTask;
            set
            {
                _selectedTask = value;
                OnPropertyChanged(nameof(SelectedTask));
            }
        }

        public Event SelectedEvent
        {
            get => _selectedEvent;
            set
            {
                _selectedEvent = value;
                OnPropertyChanged(nameof(SelectedEvent));
            }
        }

        public Note SelectedNote
        {
            get => _selectedNote;
            set
            {
                _selectedNote = value;
                OnPropertyChanged(nameof(SelectedNote));
            }
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged(nameof(SearchText));
                FilterTasks();
            }
        }

        public Tag SelectedTag
        {
            get => _selectedTag;
            set
            {
                _selectedTag = value;
                OnPropertyChanged(nameof(SelectedTag));
            }
        }

        public DateTime SelectedDate
        {
            get => _selectedDate;
            set
            {
                _selectedDate = value;
                OnPropertyChanged(nameof(SelectedDate));
            }
        }

        public bool IsDarkTheme
        {
            get => _isDarkTheme;
            set
            {
                _isDarkTheme = value;
                OnPropertyChanged(nameof(IsDarkTheme));
            }
        }

        public int CompletedTasksCount => Tasks.Count(t => t.IsCompleted);

        public ICommand AddTaskCommand { get; }
        public ICommand EditTaskCommand { get; }
        public ICommand DeleteTaskCommand { get; }
        public ICommand CompleteTaskCommand { get; }
        public ICommand AddEventCommand { get; }
        public ICommand EditEventCommand { get; }
        public ICommand DeleteEventCommand { get; }
        public ICommand AddNoteCommand { get; }
        public ICommand EditNoteCommand { get; }
        public ICommand DeleteNoteCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand ClearSearchCommand { get; }
        public ICommand ToggleThemeCommand { get; }
        public ICommand ExportToJsonCommand { get; }
        public ICommand ImportFromJsonCommand { get; }
        public ICommand ExportToCsvCommand { get; }
        public ICommand ImportFromCsvCommand { get; }
        public ICommand LogoutCommand { get; }

        public MainViewModel()
        {
            _databaseService = new DatabaseService();
            _notificationService = new NotificationService();
            Tasks = new ObservableCollection<Models.Task>();
            Events = new ObservableCollection<Event>();
            Notes = new ObservableCollection<Note>();
            Tags = new ObservableCollection<Tag>();
            SelectedDate = DateTime.Today;

            AddTaskCommand = new RelayCommand(ExecuteAddTask);
            EditTaskCommand = new RelayCommand(ExecuteEditTask, CanExecuteTaskCommand);
            DeleteTaskCommand = new RelayCommand(ExecuteDeleteTask, CanExecuteTaskCommand);
            CompleteTaskCommand = new RelayCommand(ExecuteCompleteTask, CanExecuteTaskCommand);
            AddEventCommand = new RelayCommand(ExecuteAddEvent);
            EditEventCommand = new RelayCommand(ExecuteEditEvent, CanExecuteEventCommand);
            DeleteEventCommand = new RelayCommand(ExecuteDeleteEvent, CanExecuteEventCommand);
            AddNoteCommand = new RelayCommand(ExecuteAddNote);
            EditNoteCommand = new RelayCommand(ExecuteEditNote, CanExecuteNoteCommand);
            DeleteNoteCommand = new RelayCommand(ExecuteDeleteNote, CanExecuteNoteCommand);
            SearchCommand = new RelayCommand(ExecuteSearch);
            ClearSearchCommand = new RelayCommand(ExecuteClearSearch);
            ToggleThemeCommand = new RelayCommand(ExecuteToggleTheme);
            ExportToJsonCommand = new RelayCommand(ExecuteExportToJson);
            ImportFromJsonCommand = new RelayCommand(ExecuteImportFromJson);
            ExportToCsvCommand = new RelayCommand(ExecuteExportToCsv);
            ImportFromCsvCommand = new RelayCommand(ExecuteImportFromCsv);
            LogoutCommand = new RelayCommand(ExecuteLogout);

            LoadTags();
        }

        private void StartReminderScheduler()
        {
            if (_reminderScheduler == null)
            {
                _reminderScheduler = new ReminderScheduler();
            }
            _reminderScheduler.Start();
        }

        private void LoadUserData()
        {
            try
            {
                Tasks = new ObservableCollection<Models.Task>(_databaseService.GetTasksByUserId(CurrentUser.Id));
                Events = new ObservableCollection<Event>(_databaseService.GetEventsByUserId(CurrentUser.Id));
                Notes = new ObservableCollection<Note>(_databaseService.GetNotesByUserId(CurrentUser.Id));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных пользователя: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadTags()
        {
            try
            {
                Tags = new ObservableCollection<Tag>(_databaseService.GetAllTags());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки тегов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanExecuteTaskCommand(object parameter)
        {
            return SelectedTask != null;
        }

        private bool CanExecuteEventCommand(object parameter)
        {
            return SelectedEvent != null;
        }

        private bool CanExecuteNoteCommand(object parameter)
        {
            return SelectedNote != null;
        }

        private void ExecuteAddTask(object parameter)
        {
            try
            {
                var dialog = new Views.Dialogs.TaskDialog(string.Empty, string.Empty, DateTime.Today.AddDays(1));
                dialog.Owner = Application.Current.MainWindow;
                if (dialog.ShowDialog() != true)
                {
                    return;
                }

                var task = new Models.Task
                {
                    UserId = CurrentUser.Id,
                    Title = dialog.TaskTitle,
                    Description = dialog.TaskDescription,
                    DueDate = dialog.TaskDueDate,
                    IsCompleted = false,
                    Priority = dialog.TaskPriority
                };

                bool result = _databaseService.CreateTask(task);
                if (result)
                {
                    if (dialog.ReminderEnabled && dialog.ReminderTime.HasValue)
                    {
                        var reminder = new Reminder
                        {
                            UserId = CurrentUser.Id,
                            TaskId = task.Id,
                            ReminderDate = dialog.ReminderTime.Value,
                            Message = $"Задача '{task.Title}' — срок {dialog.TaskDueDate:dd.MM.yyyy HH:mm}",
                            IsShown = true
                        };
                        _databaseService.CreateReminder(reminder);
                    }
                    LoadUserData();
                    _notificationService.ShowNotification("Задача создана", $"Задача '{task.Title}' успешно добавлена");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении задачи: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteEditTask(object parameter)
        {
            try
            {
                if (SelectedTask == null)
                {
                    return;
                }

                var dialog = new Views.Dialogs.TaskDialog(SelectedTask.Title, SelectedTask.Description, SelectedTask.DueDate, SelectedTask.Priority);
                dialog.Owner = Application.Current.MainWindow;
                if (dialog.ShowDialog() != true)
                {
                    return;
                }

                SelectedTask.Title = dialog.TaskTitle;
                SelectedTask.Description = dialog.TaskDescription;
                SelectedTask.DueDate = dialog.TaskDueDate;
                SelectedTask.Priority = dialog.TaskPriority;

                bool result = _databaseService.UpdateTask(SelectedTask);
                if (result)
                {
                    LoadUserData();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при изменении задачи: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteDeleteTask(object parameter)
        {
            try
            {
                if (SelectedTask != null && MessageBox.Show("Вы уверены, что хотите удалить эту задачу?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    bool result = _databaseService.DeleteTask(SelectedTask.Id);
                    if (result)
                    {
                        LoadUserData();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении задачи: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteCompleteTask(object parameter)
        {
            try
            {
                if (SelectedTask != null)
                {
                    SelectedTask.IsCompleted = !SelectedTask.IsCompleted;
                    bool result = _databaseService.UpdateTask(SelectedTask);
                    if (result)
                    {
                        LoadUserData();
                        _notificationService.ShowNotification("Задача обновлена", $"Задача '{SelectedTask.Title}' отмечена как {(SelectedTask.IsCompleted ? "выполненная" : "невыполненная")}");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при выполнении задачи: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteAddEvent(object parameter)
        {
            try
            {
                var dialog = new Views.Dialogs.EventDialog(string.Empty, string.Empty, string.Empty, DateTime.Today);
                dialog.Owner = Application.Current.MainWindow;
                if (dialog.ShowDialog() != true)
                {
                    return;
                }

                var ev = new Event
                {
                    UserId = CurrentUser.Id,
                    Title = dialog.EventTitle,
                    Description = dialog.EventDescription,
                    StartDate = dialog.EventStart,
                    EndDate = dialog.EventStart.AddHours(1),
                    Location = dialog.EventLocation
                };

                bool result = _databaseService.CreateEvent(ev);
                if (result)
                {
                    if (dialog.ReminderEnabled && dialog.ReminderTime.HasValue)
                    {
                        string message = $"Событие '{ev.Title}' — начало {dialog.EventStart:dd.MM.yyyy HH:mm}" + (string.IsNullOrWhiteSpace(ev.Location) ? "" : $", место: {ev.Location}");
                        _notificationService.ScheduleNotification("Напоминание", message, dialog.ReminderTime.Value);
                    }
                    LoadUserData();
                    _notificationService.ShowNotification("Событие создано", $"Событие '{ev.Title}' успешно добавлено");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении события: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteEditEvent(object parameter)
        {
            try
            {
                if (SelectedEvent == null)
                {
                    return;
                }

                var dialog = new Views.Dialogs.EventDialog(SelectedEvent.Title, SelectedEvent.Description, SelectedEvent.Location, SelectedEvent.StartDate);
                dialog.Owner = Application.Current.MainWindow;
                if (dialog.ShowDialog() != true)
                {
                    return;
                }

                SelectedEvent.Title = dialog.EventTitle;
                SelectedEvent.Description = dialog.EventDescription;
                SelectedEvent.Location = dialog.EventLocation;
                SelectedEvent.StartDate = dialog.EventStart;
                SelectedEvent.EndDate = dialog.EventStart.AddHours(1);

                bool result = _databaseService.UpdateEvent(SelectedEvent);
                if (result)
                {
                    LoadUserData();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при изменении события: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteDeleteEvent(object parameter)
        {
            try
            {
                if (SelectedEvent != null && MessageBox.Show("Вы уверены, что хотите удалить это событие?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    bool result = _databaseService.DeleteEvent(SelectedEvent.Id);
                    if (result)
                    {
                        LoadUserData();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении события: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteAddNote(object parameter)
        {
            try
            {
                var dialog = new Views.Dialogs.NoteDialog(string.Empty, string.Empty);
                dialog.Owner = Application.Current.MainWindow;
                if (dialog.ShowDialog() != true)
                {
                    return;
                }

                var note = new Note
                {
                    UserId = CurrentUser.Id,
                    Title = dialog.NoteTitle,
                    Content = dialog.NoteContent,
                    CreatedDate = DateTime.Now
                };

                bool result = _databaseService.CreateNote(note);
                if (result)
                {
                    LoadUserData();
                    _notificationService.ShowNotification("Заметка создана", $"Заметка '{note.Title}' успешно добавлена");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении заметки: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteEditNote(object parameter)
        {
            try
            {
                if (SelectedNote == null)
                {
                    return;
                }

                var dialog = new Views.Dialogs.NoteDialog(SelectedNote.Title, SelectedNote.Content);
                dialog.Owner = Application.Current.MainWindow;
                if (dialog.ShowDialog() != true)
                {
                    return;
                }

                SelectedNote.Title = dialog.NoteTitle;
                SelectedNote.Content = dialog.NoteContent;

                bool result = _databaseService.UpdateNote(SelectedNote);
                if (result)
                {
                    LoadUserData();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при изменении заметки: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteDeleteNote(object parameter)
        {
            try
            {
                if (SelectedNote != null && MessageBox.Show("Вы уверены, что хотите удалить эту заметку?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    bool result = _databaseService.DeleteNote(SelectedNote.Id);
                    if (result)
                    {
                        LoadUserData();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении заметки: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteSearch(object parameter)
        {
            FilterTasks();
        }

        private void ExecuteClearSearch(object parameter)
        {
            SearchText = string.Empty;
        }

        private void FilterTasks()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(SearchText))
                {
                    Tasks = new ObservableCollection<Models.Task>(_databaseService.GetTasksByUserId(CurrentUser.Id));
                }
                else
                {
                    var filteredTasks = _databaseService.GetTasksByUserId(CurrentUser.Id)
                        .Where(t => (t.Title != null && t.Title.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) >= 0) ||
                                   (t.Description != null && t.Description.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) >= 0))
                        .ToList();
                    Tasks = new ObservableCollection<Models.Task>(filteredTasks);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при фильтрации задач: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteToggleTheme(object parameter)
        {
            IsDarkTheme = !IsDarkTheme;
            App.ApplyTheme(IsDarkTheme);
        }

        private void ExecuteExportToJson(object parameter)
        {
            try
            {
                var tasks = _databaseService.GetTasksByUserId(CurrentUser.Id);
                string json = JsonConvert.SerializeObject(tasks, Formatting.Indented);

                var saveFileDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                    DefaultExt = ".json"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    File.WriteAllText(saveFileDialog.FileName, json);
                    MessageBox.Show("Задачи успешно экспортированы в JSON!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при экспорте в JSON: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteImportFromJson(object parameter)
        {
            try
            {
                var openFileDialog = new Microsoft.Win32.OpenFileDialog
                {
                    Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*"
                };

                if (openFileDialog.ShowDialog() == true)
                {
                    string json = File.ReadAllText(openFileDialog.FileName);
                    var tasks = JsonConvert.DeserializeObject<List<Models.Task>>(json);

                    if (tasks != null)
                    {
                        foreach (var task in tasks)
                        {
                            task.UserId = CurrentUser.Id;
                            _databaseService.CreateTask(task);
                        }
                        LoadUserData();
                        MessageBox.Show("Задачи успешно импортированы из JSON!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при импорте из JSON: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteExportToCsv(object parameter)
        {
            try
            {
                var tasks = _databaseService.GetTasksByUserId(CurrentUser.Id);

                var saveFileDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
                    DefaultExt = ".csv"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    using (var writer = new StreamWriter(saveFileDialog.FileName))
                    using (var csv = new CsvWriter(writer, System.Globalization.CultureInfo.CurrentCulture))
                    {
                        csv.WriteRecords(tasks);
                    }
                    MessageBox.Show("Задачи успешно экспортированы в CSV!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при экспорте в CSV: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteImportFromCsv(object parameter)
        {
            try
            {
                var openFileDialog = new Microsoft.Win32.OpenFileDialog
                {
                    Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*"
                };

                if (openFileDialog.ShowDialog() == true)
                {
                    using (var reader = new StreamReader(openFileDialog.FileName))
                    using (var csv = new CsvReader(reader, System.Globalization.CultureInfo.CurrentCulture))
                    {
                        var tasks = csv.GetRecords<Models.Task>().ToList();
                        foreach (var task in tasks)
                        {
                            task.UserId = CurrentUser.Id;
                            _databaseService.CreateTask(task);
                        }
                        LoadUserData();
                        MessageBox.Show("Задачи успешно импортированы из CSV!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при импорте из CSV: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteLogout(object parameter)
        {
            try
            {
                _reminderScheduler?.Stop();
                CurrentUser = null;
                Tasks.Clear();
                Events.Clear();
                Notes.Clear();

                var loginPage = new Views.LoginWindow();
                var loginHost = new System.Windows.Navigation.NavigationWindow
                {
                    Content = loginPage,
                    ShowsNavigationUI = false,
                    Title = "Ежедневник — вход",
                    Width = 1000,
                    Height = 650,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen
                };
                loginHost.Show();
                Application.Current.MainWindow = loginHost;

                foreach (System.Windows.Window window in Application.Current.Windows)
                {
                    if (window != loginHost)
                    {
                        if (window is MainWindow mainWindow)
                        {
                            mainWindow.AllowClose = true;
                        }
                        window.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при выходе: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
