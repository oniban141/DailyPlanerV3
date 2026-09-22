using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DailyPlaner.Services
{
    public class NotificationService
    {
        public void ShowNotification(string title, string message)
        {
            try
            {
                using (var toast = new System.Windows.Forms.NotifyIcon())
                {
                    toast.BalloonTipTitle = title;
                    toast.BalloonTipText = message;
                    toast.BalloonTipIcon = System.Windows.Forms.ToolTipIcon.Info;
                    toast.Visible = true;
                    toast.ShowBalloonTip(3000);
                }
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
                string fullMessage = $"{message}\n\nDue: {dueDate.ToString(\"yyyy-MM-dd HH:mm\")}";
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
                var timer = new System.Threading.Timer(_ =>
                {
                    ShowNotification(title, message);
                }, null, scheduleTime - DateTime.Now, TimeSpan.FromMilliseconds(-1));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error scheduling notification: {ex.Message}");
            }
        }
    }
}
