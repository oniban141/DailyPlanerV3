using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace DailyPlaner.Services
{
    public class NotificationService
    {
        private static readonly List<System.Threading.Timer> Timers = new List<System.Threading.Timer>();

        public void ShowNotification(string title, string message)
        {
            try
            {
                var dispatcher = System.Windows.Application.Current?.Dispatcher;
                if (dispatcher != null && !dispatcher.CheckAccess())
                {
                    dispatcher.BeginInvoke((Action)(() => ShowNotification(title, message)));
                    return;
                }

                var icon = App.TrayIcon;
                if (icon == null || !icon.Visible)
                {
                    return;
                }

                icon.BalloonTipTitle = title;
                icon.BalloonTipText = message;
                icon.BalloonTipIcon = System.Windows.Forms.ToolTipIcon.Info;
                icon.ShowBalloonTip(4000);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error showing notification: {ex.Message}");
            }
        }

        public void ShowReminderNotification(string title, string message, DateTime dueDate)
        {
            try
            {
                string fullMessage = $"{message}\n\nDue: {dueDate:yyyy-MM-dd HH:mm}";
                ShowNotification(title, fullMessage);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error showing reminder: {ex.Message}");
            }
        }

        public void ScheduleNotification(string title, string message, DateTime scheduleTime)
        {
            try
            {
                var delay = scheduleTime - DateTime.Now;
                if (delay < TimeSpan.Zero)
                {
                    ShowNotification(title, message);
                    return;
                }

                System.Threading.Timer timer = null;
                timer = new System.Threading.Timer(_ =>
                {
                    try
                    {
                        ShowNotification(title, message);
                    }
                    finally
                    {
                        lock (Timers)
                        {
                            Timers.Remove(timer);
                        }
                        timer?.Dispose();
                    }
                }, null, delay, TimeSpan.FromMilliseconds(-1));

                lock (Timers)
                {
                    Timers.Add(timer);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error scheduling notification: {ex.Message}");
            }
        }
    }}
