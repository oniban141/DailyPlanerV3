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

namespace DailyPlaner
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
        }

        public MainWindow(Models.User user)
        {
            InitializeComponent();
            var viewModel = new MainViewModel();
            DataContext = viewModel;
            viewModel.CurrentUser = user;
        }

        private void SidebarNav_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is Button button) || !(button.Tag is string pageName))
            {
                return;
            }

            ShowPage(pageName);
        }

        private void ShowPage(string pageName)
        {
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
        }
    }
}
