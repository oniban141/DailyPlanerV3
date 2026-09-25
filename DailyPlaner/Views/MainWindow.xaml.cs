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
using DailyPlaner.Models;

namespace DailyPlaner
{
    public partial class MainWindow : Window
    {
        public bool AllowClose { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            App.SetWindowIcon(this);
            DataContext = new MainViewModel();
            ((MainViewModel)DataContext).ActivePage = "OverviewPage";
            AutoStartSidebarCheckBox.IsChecked = App.LoadAutoStartSetting();
            UpdateToolbarForPage("OverviewPage");
            Loaded += (s, e) => RefreshDayLists();
        }

        public MainWindow(Models.User user)
        {
            InitializeComponent();
            App.SetWindowIcon(this);
            var viewModel = new MainViewModel();
            DataContext = viewModel;
            viewModel.CurrentUser = user;
            viewModel.ActivePage = "OverviewPage";
            AutoStartSidebarCheckBox.IsChecked = App.LoadAutoStartSetting();
            UpdateToolbarForPage("OverviewPage");
            viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(MainViewModel.SelectedDate) || e.PropertyName == nameof(MainViewModel.Tasks) || e.PropertyName == nameof(MainViewModel.Events))
                {
                    RefreshDayLists();
                }
            };
            Loaded += (s, e) => RefreshDayLists();
        }

        private void SidebarNav_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is Button button) || !(button.Tag is string pageName))
            {
                return;
            }

            ShowPage(pageName);
            if (pageName == "CalendarPage")
            {
                RefreshDayLists();
            }
        }

        private void ShowPage(string pageName)
        {
            if (DataContext is MainViewModel vm)
            {
                vm.ActivePage = pageName;
                vm.ResetFiltersAndReload();
            }

            OverviewPage.Visibility = pageName == "OverviewPage" ? Visibility.Visible : Visibility.Collapsed;
            TasksPage.Visibility = pageName == "TasksPage" ? Visibility.Visible : Visibility.Collapsed;
            EventsPage.Visibility = pageName == "EventsPage" ? Visibility.Visible : Visibility.Collapsed;
            NotesPage.Visibility = pageName == "NotesPage" ? Visibility.Visible : Visibility.Collapsed;
            CalendarPage.Visibility = pageName == "CalendarPage" ? Visibility.Visible : Visibility.Collapsed;

            NavOverviewButton.Style = (Style)FindResource(pageName == "OverviewPage" ? "SidebarActiveButtonStyle" : "SidebarButtonStyle");
            NavTasksButton.Style = (Style)FindResource(pageName == "TasksPage" ? "SidebarActiveButtonStyle" : "SidebarButtonStyle");
            NavEventsButton.Style = (Style)FindResource(pageName == "EventsPage" ? "SidebarActiveButtonStyle" : "SidebarButtonStyle");
            NavNotesButton.Style = (Style)FindResource(pageName == "NotesPage" ? "SidebarActiveButtonStyle" : "SidebarButtonStyle");
            NavCalendarButton.Style = (Style)FindResource(pageName == "CalendarPage" ? "SidebarActiveButtonStyle" : "SidebarButtonStyle");

            UpdateToolbarForPage(pageName);
        }

        private void UpdateToolbarForPage(string pageName)
        {
            bool isOverview = pageName == "OverviewPage";
            bool isCalendar = pageName == "CalendarPage";

            ToolbarBorder.Visibility = isCalendar ? Visibility.Collapsed : Visibility.Visible;
            SearchScopeComboBox.Visibility = isOverview ? Visibility.Visible : Visibility.Collapsed;
            SearchTextBoxHost.Visibility = isCalendar ? Visibility.Collapsed : Visibility.Visible;
            SearchButton.Visibility = isCalendar ? Visibility.Collapsed : Visibility.Visible;
            ClearSearchButton.Visibility = isCalendar ? Visibility.Collapsed : Visibility.Visible;
            ToolbarDatePicker.Visibility = isOverview ? Visibility.Collapsed : Visibility.Visible;

            if (isCalendar)
            {
                return;
            }

            SearchTextBox.Tag = isOverview
                ? "🔍  Поиск..."
                : "🔍  Поиск по дате (дд.ММ.гггг)...";
        }

        private void AutoStartSidebar_Changed(object sender, RoutedEventArgs e)
        {
            bool enabled = AutoStartSidebarCheckBox.IsChecked == true;
            App.SetAutoStart(enabled);
        }

        private void RefreshDayLists()
        {
            if (!(DataContext is MainViewModel vm) || vm.CurrentUser == null)
            {
                return;
            }

            var date = vm.SelectedDate.Date;
            DayEventsList.ItemsSource = vm.Events.Where(ev => ev.StartDate.Date == date).OrderBy(ev => ev.StartDate).ToList();
            DayTasksList.ItemsSource = vm.Tasks.Where(t => t.DueDate.Date == date).OrderBy(t => t.DueDate).ToList();
        }

        private void DayItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            object item = (sender as ListView)?.SelectedItem;
            if (item == null)
            {
                return;
            }

            if (item is Event ev)
            {
                ShowEventDetails(ev);
            }
            else if (item is Models.Task task)
            {
                ShowTaskDetails(task);
            }
        }

        private void TasksList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if ((sender as ListView)?.SelectedItem is Models.Task task)
            {
                ShowTaskDetails(task);
            }
        }

        private void EventsList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if ((sender as ListView)?.SelectedItem is Event ev)
            {
                ShowEventDetails(ev);
            }
        }

        private void NotesList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if ((sender as ListView)?.SelectedItem is Note note)
            {
                ShowNoteDetails(note);
            }
        }

        private void ShowNoteDetails(Note note)
        {
            string content = string.IsNullOrWhiteSpace(note.Content) ? "—" : note.Content;
            MessageBox.Show(
                $"Заметка: {note.Title}\n\n" +
                $"Содержимое:\n{content}\n\n" +
                $"Создано: {note.CreatedDate:dd.MM.yyyy HH:mm}",
                "Подробная информация", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ShowTaskDetails(Models.Task task)
        {
            string status = task.IsCompleted ? "Выполнена" : "Ожидает";
            string description = string.IsNullOrWhiteSpace(task.Description) ? "—" : task.Description;
            MessageBox.Show(
                $"Задача: {task.Title}\n\n" +
                $"Описание: {description}\n" +
                $"Срок: {task.DueDate:dd.MM.yyyy HH:mm}\n" +
                $"Приоритет: {task.Priority}\n" +
                $"Статус: {status}",
                "Подробная информация", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ShowEventDetails(Event ev)
        {
            string description = string.IsNullOrWhiteSpace(ev.Description) ? "—" : ev.Description;
            string location = string.IsNullOrWhiteSpace(ev.Location) ? "—" : ev.Location;
            MessageBox.Show(
                $"Событие: {ev.Title}\n\n" +
                $"Описание: {description}\n" +
                $"Начало: {ev.StartDate:dd.MM.yyyy HH:mm}\n" +
                $"Окончание: {ev.EndDate:dd.MM.yyyy HH:mm}\n" +
                $"Место: {location}",
                "Подробная информация", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            if (!AllowClose && !App.IsExiting)
            {
                e.Cancel = true;
                Hide();
                App.TrayIcon?.ShowBalloonTip(3000, "Ежедневник",
                    "Приложение свернуто в трей и продолжает работу. Дважды щёлкните по значку, чтобы открыть.",
                    System.Windows.Forms.ToolTipIcon.Info);
            }
            base.OnClosing(e);
        }
    }
}
