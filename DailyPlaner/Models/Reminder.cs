using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DailyPlaner.Models
{
    public class Reminder
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public int TaskId { get; set; }
        public Task Task { get; set; }
        public DateTime ReminderDate { get; set; }
        public string Message { get; set; }
        public bool IsShown { get; set; }
    }
}
