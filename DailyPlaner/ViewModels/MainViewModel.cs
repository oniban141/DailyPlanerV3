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
                MessageBox.Show($"Error loading user data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                MessageBox.Show($"Error loading tags: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                var task = new Models.Task
                {
                    UserId = CurrentUser.Id,
                    Title = "New Task",
                    Description = string.Empty,
                    DueDate = DateTime.Today.AddDays(1),
                    IsCompleted = false,
                    Priority = 1
                };

                bool result = _databaseService.CreateTask(task);
                if (result)
                {
                    LoadUserData();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding task: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteEditTask(object parameter)
        {
            try
            {
                if (SelectedTask != null)
                {
                    bool result = _databaseService.UpdateTask(SelectedTask);
                    if (result)
                    {
                        LoadUserData();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error editing task: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteDeleteTask(object parameter)
        {
            try
            {
                if (SelectedTask != null && MessageBox.Show("Are you sure you want to delete this task?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
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
                MessageBox.Show($"Error deleting task: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                        _notificationService.ShowNotification("Task Updated", $"Task '{SelectedTask.Title}' marked as {(SelectedTask.IsCompleted ? "completed" : "incomplete")}");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error completing task: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteAddEvent(object parameter)
        {
            try
            {
                var ev = new Event
                {
                    UserId = CurrentUser.Id,
                    Title = "New Event",
                    Description = string.Empty,
                    StartDate = DateTime.Today,
                    EndDate = DateTime.Today.AddHours(1),
                    Location = string.Empty
                };

                bool result = _databaseService.CreateEvent(ev);
                if (result)
                {
                    LoadUserData();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding event: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteEditEvent(object parameter)
        {
            try
            {
                if (SelectedEvent != null)
                {
                    bool result = _databaseService.UpdateEvent(SelectedEvent);
                    if (result)
                    {
                        LoadUserData();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error editing event: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteDeleteEvent(object parameter)
        {
            try
            {
                if (SelectedEvent != null && MessageBox.Show("Are you sure you want to delete this event?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
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
                MessageBox.Show($"Error deleting event: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteAddNote(object parameter)
        {
            try
            {
                var note = new Note
                {
                    UserId = CurrentUser.Id,
                    Title = "New Note",
                    Content = string.Empty,
                    CreatedDate = DateTime.Now
                };

                bool result = _databaseService.CreateNote(note);
                if (result)
                {
                    LoadUserData();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding note: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteEditNote(object parameter)
        {
            try
            {
                if (SelectedNote != null)
                {
                    bool result = _databaseService.UpdateNote(SelectedNote);
                    if (result)
                    {
                        LoadUserData();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error editing note: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteDeleteNote(object parameter)
        {
            try
            {
                if (SelectedNote != null && MessageBox.Show("Are you sure you want to delete this note?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
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
                MessageBox.Show($"Error deleting note: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                MessageBox.Show($"Error filtering tasks: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteToggleTheme(object parameter)
        {
            IsDarkTheme = !IsDarkTheme;
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
                    MessageBox.Show("Tasks exported to JSON successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting to JSON: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                        MessageBox.Show("Tasks imported from JSON successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error importing from JSON: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                    MessageBox.Show("Tasks exported to CSV successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting to CSV: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                        MessageBox.Show("Tasks imported from CSV successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error importing from CSV: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteLogout(object parameter)
        {
            try
            {
                CurrentUser = null;
                Tasks.Clear();
                Events.Clear();
                Notes.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during logout: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
