using System;
using System.Windows;

namespace DailyPlaner.Views.Dialogs
{
    public partial class NoteDialog : Window
    {
        public string NoteTitle => TitleBox.Text.Trim();
        public string NoteContent => ContentBox.Text.Trim();

        public NoteDialog(string title, string content)
        {
            InitializeComponent();
            TitleBox.Text = title ?? string.Empty;
            ContentBox.Text = content ?? string.Empty;
            Loaded += (s, e) => TitleBox.Focus();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NoteTitle))
            {
                MessageBox.Show("Введите название заметки.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
