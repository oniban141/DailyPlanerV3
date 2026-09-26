using System;

namespace DailyPlaner.Models
{
    public class Reminder
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int TaskId { get; set; }
        public DateTime ReminderDate { get; set; }
        public string Message { get; set; }
        public bool IsShown { get; set; }
    }
}
