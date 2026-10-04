using System;
using System.Data.Entity;
using DailyPlaner.Models;

namespace DailyPlaner.Services
{
    public class PlannerDbContext : DbContext
    {
        public PlannerDbContext()
            : base("name=DailyPlannerConnection")
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Gender> Genders { get; set; }
        public DbSet<Models.Task> Tasks { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Note> Notes { get; set; }
        public DbSet<Reminder> Reminders { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .ToTable("Users")
                .HasKey(u => u.Id);

            modelBuilder.Entity<User>()
                .Property(u => u.CreatedAt).HasColumnName("CreatedAt");

            modelBuilder.Entity<Gender>()
                .ToTable("Genders")
                .HasKey(g => g.Id);

            modelBuilder.Entity<Models.Task>()
                .ToTable("Tasks")
                .HasKey(t => t.Id);

            modelBuilder.Entity<Event>()
                .ToTable("Events")
                .HasKey(e => e.Id);

            modelBuilder.Entity<Note>()
                .ToTable("Notes")
                .HasKey(n => n.Id);
            modelBuilder.Entity<Note>()
                .Property(n => n.CreatedDate).HasColumnName("CreatedAt");

            modelBuilder.Entity<Reminder>()
                .ToTable("Reminders")
                .HasKey(r => r.Id);
            modelBuilder.Entity<Reminder>()
                .Property(r => r.TaskId).IsOptional();
            modelBuilder.Entity<Reminder>()
                .Property(r => r.ReminderDate).HasColumnName("ReminderTime");
            modelBuilder.Entity<Reminder>()
                .Property(r => r.IsShown).HasColumnName("IsActive");
        }
    }
}
