using System;
using System.Windows;
using DailyPlaner.Models;

namespace DailyPlaner.Views.Dialogs
{
    public partial class DetailsDialog : Window
    {
        private DetailsDialog()
        {
            InitializeComponent();
        }

        public static void ShowTask(System.Windows.Window owner, Models.Task task)
        {
            var dialog = new DetailsDialog
            {
                Owner = owner
            };
            dialog.KindIcon.Text = "📋";
            dialog.KindLabel.Text = "Задача";
            dialog.TitleText.Text = string.IsNullOrWhiteSpace(task.Title) ? "—" : task.Title;
            dialog.DescriptionText.Text = string.IsNullOrWhiteSpace(task.Description) ? "—" : task.Description;
            dialog.TimeLabel.Text = "Срок выполнения";
            dialog.TimeText.Text = task.DueDate.ToString("dd.MM.yyyy HH:mm");
            dialog.ExtraLabel.Text = "Приоритет";
            dialog.ExtraText.Text = string.IsNullOrWhiteSpace(task.Priority) ? "—" : task.Priority;
            dialog.StatusLabel.Text = "Статус";
            dialog.StatusText.Text = task.IsCompleted ? "✅ Выполнена" : "🕒 Ожидает выполнения";
            dialog.ShowDialog();
        }

        public static void ShowEvent(System.Windows.Window owner, Event ev)
        {
            var dialog = new DetailsDialog
            {
                Owner = owner
            };
            dialog.KindIcon.Text = "📅";
            dialog.KindLabel.Text = "Событие";
            dialog.TitleText.Text = string.IsNullOrWhiteSpace(ev.Title) ? "—" : ev.Title;
            dialog.DescriptionText.Text = string.IsNullOrWhiteSpace(ev.Description) ? "—" : ev.Description;
            dialog.TimeLabel.Text = "Начало и окончание";
            dialog.TimeText.Text = $"{ev.StartDate:dd.MM.yyyy HH:mm} — {ev.EndDate:dd.MM.yyyy HH:mm}";
            dialog.ExtraLabel.Text = "Место";
            dialog.ExtraText.Text = string.IsNullOrWhiteSpace(ev.Location) ? "—" : ev.Location;
            dialog.StatusLabel.Visibility = System.Windows.Visibility.Collapsed;
            dialog.StatusText.Visibility = System.Windows.Visibility.Collapsed;
            dialog.ShowDialog();
        }

        public static void ShowNote(System.Windows.Window owner, Note note)
        {
            var dialog = new DetailsDialog
            {
                Owner = owner
            };
            dialog.KindIcon.Text = "📝";
            dialog.KindLabel.Text = "Заметка";
            dialog.TitleText.Text = string.IsNullOrWhiteSpace(note.Title) ? "—" : note.Title;
            dialog.DescriptionText.Text = string.IsNullOrWhiteSpace(note.Content) ? "—" : note.Content;
            dialog.TimeLabel.Text = "Создано";
            dialog.TimeText.Text = note.CreatedDate.ToString("dd.MM.yyyy HH:mm");
            dialog.ExtraLabel.Visibility = System.Windows.Visibility.Collapsed;
            dialog.ExtraText.Visibility = System.Windows.Visibility.Collapsed;
            dialog.StatusLabel.Visibility = System.Windows.Visibility.Collapsed;
            dialog.StatusText.Visibility = System.Windows.Visibility.Collapsed;
            dialog.ShowDialog();
        }

        private void Close_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Close();
        }
    }
}
