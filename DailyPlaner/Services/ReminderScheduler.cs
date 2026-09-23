using System;
using System.Collections.Generic;
using DailyPlaner.Models;

namespace DailyPlaner.Services
{
    public class ReminderScheduler
    {
        private readonly DatabaseService _databaseService;
        private readonly NotificationService _notificationService;
        private readonly System.Windows.Threading.DispatcherTimer _timer;
        private readonly HashSet<int> _shownReminderIds;

        public ReminderScheduler()
        {
            _databaseService = new DatabaseService();
            _notificationService = new NotificationService();
            _shownReminderIds = new HashSet<int>();
            _timer = new System.Windows.Threading.DispatcherTimer
            {
                Interval = TimeSpan.FromMinutes(1)
            };
            _timer.Tick += (s, e) => CheckReminders();
        }

        public void Start()
        {
            _timer.Start();
        }

        public void Stop()
        {
            _timer.Stop();
        }

        public void CheckReminders()
        {
            try
            {
                var reminders = _databaseService.GetAllReminders();
                var now = DateTime.Now;

                foreach (var reminder in reminders)
                {
                    if (reminder == null || !reminder.IsShown || _shownReminderIds.Contains(reminder.Id))
                    {
                        continue;
                    }

                    if (reminder.ReminderDate <= now && reminder.ReminderDate > now.AddHours(-12))
                    {
                        _shownReminderIds.Add(reminder.Id);
                        reminder.IsShown = false;
                        _databaseService.UpdateReminder(reminder);
                        string message = string.IsNullOrWhiteSpace(reminder.Message)
                            ? "Скоро запланированное дело"
                            : reminder.Message;
                        _notificationService.ShowReminderNotification("Напоминание", message, reminder.ReminderDate);
                    }
                }
            }
            catch (Exception)
            {
            }
        }
    }
}
