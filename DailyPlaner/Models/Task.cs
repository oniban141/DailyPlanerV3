using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DailyPlaner.Models
{
    public class Task
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime DueDate { get; set; }
        public string Status { get; set; }
        public string Priority { get; set; } = "Средний";
        public bool IsCompleted
        {
            get => Status == "Выполнена" || Status == "Completed";
            set => Status = value ? "Выполнена" : "Ожидает";
        }
    }
}
