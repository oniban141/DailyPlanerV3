using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using DailyPlaner.Models;

namespace DailyPlaner.Services
{
    public class DatabaseService
    {
        private readonly string connectionString;

        public DatabaseService()
        {
            string configured = System.Configuration.ConfigurationManager.ConnectionStrings["DailyPlannerConnection"]?.ConnectionString;
            connectionString = string.IsNullOrWhiteSpace(configured)
                ? "Server=PCGl1tch;Database=DailyPlannerDB;Trusted_Connection=True;TrustServerCertificate=True;"
                : configured;
        }

        public DatabaseService(string customConnectionString)
        {
            connectionString = customConnectionString;
        }

        public static string GetFriendlyDatabaseError(Exception exception)
        {
            var sqlEx = exception as SqlException;
            if (sqlEx == null)
            {
                return $"Ошибка базы данных: {exception.Message}";
            }
            switch (sqlEx.Number)
            {
                case -1:
                case 2:
                case 53:
                    return "Сервер 'PCGl1tch' недоступен. Проверьте, что SQL Server запущен, и имя сервера указано верно (Server=PCGl1tch).";
                case 4060:
                    return "База данных 'DailyPlannerDB' не существует или недоступна. Выполните скрипт Database/DailyPlannerDB.sql для её создания.";
                case 18456:
                    return "Ошибка авторизации Windows. Проверьте, что учётная запись Windows имеет доступ к SQL Server.";
                case 18452:
                    return "Неудачная попытка входа. Проверьте режим аутентификации SQL Server (Windows Authentication).";
                default:
                    return $"Ошибка базы данных (код {sqlEx.Number}): {sqlEx.Message}";
            }
        }

        public static string HashPassword(string password)
        {
            using (var sha = System.Security.Cryptography.SHA256.Create())
            {
                byte[] bytes = sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                var builder = new System.Text.StringBuilder();
                foreach (byte b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }

        public bool TestConnection()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    EnsureDatabaseIsReady(connection);
                    return true;
                }
                catch (SqlException ex)
                {
                    throw new InvalidOperationException(GetFriendlyDatabaseError(ex), ex);
                }
            }
        }

        private void EnsureDatabaseIsReady(SqlConnection connection)
        {
            try
            {
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "IF COL_LENGTH('dbo.Events', 'Status') IS NULL ALTER TABLE dbo.Events ADD Status NVARCHAR(50) NULL";
                    command.ExecuteNonQuery();
                    command.CommandText = "IF EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CHK_Events_Status') ALTER TABLE dbo.Events DROP CONSTRAINT CHK_Events_Status";
                    command.ExecuteNonQuery();
                    command.CommandText = "UPDATE Events SET Status = N'Запланировано' WHERE Status IS NULL OR Status NOT IN (N'Запланировано', N'Завершено')";
                    command.ExecuteNonQuery();
                    command.CommandText = "ALTER TABLE dbo.Events ADD CONSTRAINT CHK_Events_Status CHECK (Status IN (N'Запланировано', N'Завершено'))";
                    command.ExecuteNonQuery();
                    command.CommandText = @"IF EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CHK_Tasks_Priority')
    ALTER TABLE dbo.Tasks DROP CONSTRAINT CHK_Tasks_Priority;
IF EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CHK_Tasks_Status')
    ALTER TABLE dbo.Tasks DROP CONSTRAINT CHK_Tasks_Status;
UPDATE Tasks
SET Priority = CASE Priority
    WHEN '1' THEN N'Низкий'
    WHEN '2' THEN N'Средний'
    WHEN '3' THEN N'Высокий'
    ELSE Priority
END
WHERE Priority IN ('1', '2', '3');
UPDATE Tasks
SET Status = CASE
    WHEN Status LIKE N'%ыполнен%' OR Status LIKE N'Completed%' OR Status LIKE N'Done%' OR Status LIKE N'Готово%' THEN N'Выполнена'
    ELSE N'Ожидает'
END
WHERE Status IS NULL OR Status NOT IN (N'Ожидает', N'Выполнена');
ALTER TABLE dbo.Tasks ADD CONSTRAINT CHK_Tasks_Priority CHECK (Priority IN (N'Низкий', N'Средний', N'Высокий'));
ALTER TABLE dbo.Tasks ADD CONSTRAINT CHK_Tasks_Status CHECK (Status IN (N'Ожидает', N'Выполнена'));";
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Проверка базы данных: {ex.Message}");
            }
        }

        public User GetUserByUsername(string username)
        {
            var users = Query("SELECT * FROM Users WHERE Username = @Username",
                command => command.Parameters.AddWithValue("@Username", username), ReadUser);
            return users.Count > 0 ? users[0] : null;
        }

        public bool UsernameExists(string username)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand("SELECT COUNT(1) FROM Users WHERE Username = @Username", connection))
                    {
                        command.Parameters.AddWithValue("@Username", username);
                        return Convert.ToInt32(command.ExecuteScalar()) > 0;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    return false;
                }
            }
        }

        public bool CreateUser(User user)
        {
            return Execute("INSERT INTO Users (Username, PasswordHash, Email, GenderId, RoleId, CreatedAt) " +
                           "VALUES (@Username, @PasswordHash, @Email, @GenderId, @RoleId, @CreatedAt)", command =>
            {
                command.Parameters.AddWithValue("@Username", user.Username);
                command.Parameters.AddWithValue("@PasswordHash", user.PasswordHash);
                command.Parameters.AddWithValue("@Email", user.Email ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@GenderId", user.GenderId);
                command.Parameters.AddWithValue("@RoleId", user.RoleId);
                command.Parameters.AddWithValue("@CreatedAt", user.CreatedAt);
            }, true);
        }

        public List<Gender> GetAllGenders()
        {
            return Query("SELECT * FROM Genders", null, reader => new Gender
            {
                Id = Convert.ToInt32(reader["Id"]),
                Name = reader["Name"].ToString()
            });
        }

        public List<Models.Task> GetTasksByUserId(int userId)
        {
            return Query("SELECT * FROM Tasks WHERE UserId = @UserId",
                command => command.Parameters.AddWithValue("@UserId", userId), ReadTask);
        }

        public bool CreateTask(Models.Task task)
        {
            var result = Scalar("INSERT INTO Tasks (UserId, Title, Description, DueDate, Priority, Status) " +
                                "VALUES (@UserId, @Title, @Description, @DueDate, @Priority, @Status); SELECT CAST(SCOPE_IDENTITY() AS INT);", command =>
            {
                command.Parameters.AddWithValue("@UserId", task.UserId);
                command.Parameters.AddWithValue("@Title", task.Title);
                command.Parameters.AddWithValue("@Description", task.Description ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@DueDate", task.DueDate);
                command.Parameters.AddWithValue("@Priority", (object)task.Priority ?? "Средний");
                command.Parameters.AddWithValue("@Status", task.IsCompleted ? "Выполнена" : "Ожидает");
            }, true);
            if (result == null || !int.TryParse(result.ToString(), out int newId))
            {
                return false;
            }
            task.Id = newId;
            return true;
        }

        public bool UpdateTask(Models.Task task)
        {
            return Execute("UPDATE Tasks SET UserId = @UserId, Title = @Title, Description = @Description, " +
                           "DueDate = @DueDate, Priority = @Priority, Status = @Status WHERE Id = @Id", command =>
            {
                command.Parameters.AddWithValue("@Id", task.Id);
                command.Parameters.AddWithValue("@UserId", task.UserId);
                command.Parameters.AddWithValue("@Title", task.Title);
                command.Parameters.AddWithValue("@Description", task.Description ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@DueDate", task.DueDate);
                command.Parameters.AddWithValue("@Priority", (object)task.Priority ?? "Средний");
                command.Parameters.AddWithValue("@Status", task.IsCompleted ? "Выполнена" : "Ожидает");
            });
        }

        public bool DeleteTask(int id)
        {
            Execute("DELETE FROM Reminders WHERE TaskId = @Id",
                command => command.Parameters.AddWithValue("@Id", id));
            return Execute("DELETE FROM Tasks WHERE Id = @Id",
                command => command.Parameters.AddWithValue("@Id", id));
        }

        public List<Event> GetEventsByUserId(int userId)
        {
            return Query("SELECT * FROM Events WHERE UserId = @UserId",
                command => command.Parameters.AddWithValue("@UserId", userId), ReadEvent);
        }

        public bool CreateEvent(Event ev)
        {
            return Execute("INSERT INTO Events (UserId, Title, Description, StartDate, EndDate, Location, Status) " +
                           "VALUES (@UserId, @Title, @Description, @StartDate, @EndDate, @Location, @Status)", command =>
            {
                command.Parameters.AddWithValue("@UserId", ev.UserId);
                command.Parameters.AddWithValue("@Title", ev.Title);
                command.Parameters.AddWithValue("@Description", ev.Description ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@StartDate", ev.StartDate);
                command.Parameters.AddWithValue("@EndDate", ev.EndDate);
                command.Parameters.AddWithValue("@Location", ev.Location ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Status", ev.IsCompleted ? "Завершено" : "Запланировано");
            }, true);
        }

        public bool UpdateEvent(Event ev)
        {
            return Execute("UPDATE Events SET UserId = @UserId, Title = @Title, Description = @Description, " +
                           "StartDate = @StartDate, EndDate = @EndDate, Location = @Location, Status = @Status WHERE Id = @Id", command =>
            {
                command.Parameters.AddWithValue("@Id", ev.Id);
                command.Parameters.AddWithValue("@UserId", ev.UserId);
                command.Parameters.AddWithValue("@Title", ev.Title);
                command.Parameters.AddWithValue("@Description", ev.Description ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@StartDate", ev.StartDate);
                command.Parameters.AddWithValue("@EndDate", ev.EndDate);
                command.Parameters.AddWithValue("@Location", ev.Location ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Status", ev.IsCompleted ? "Завершено" : "Запланировано");
            });
        }

        public bool DeleteEvent(int id)
        {
            return Execute("DELETE FROM Events WHERE Id = @Id",
                command => command.Parameters.AddWithValue("@Id", id));
        }

        public List<Note> GetNotesByUserId(int userId)
        {
            return Query("SELECT * FROM Notes WHERE UserId = @UserId",
                command => command.Parameters.AddWithValue("@UserId", userId), ReadNote);
        }

        public bool CreateNote(Note note)
        {
            return Execute("INSERT INTO Notes (UserId, Title, Content, CreatedAt) " +
                           "VALUES (@UserId, @Title, @Content, @CreatedAt)", command =>
            {
                command.Parameters.AddWithValue("@UserId", note.UserId);
                command.Parameters.AddWithValue("@Title", note.Title);
                command.Parameters.AddWithValue("@Content", note.Content);
                command.Parameters.AddWithValue("@CreatedAt", note.CreatedDate);
            }, true);
        }

        public bool UpdateNote(Note note)
        {
            return Execute("UPDATE Notes SET UserId = @UserId, Title = @Title, Content = @Content WHERE Id = @Id", command =>
            {
                command.Parameters.AddWithValue("@Id", note.Id);
                command.Parameters.AddWithValue("@UserId", note.UserId);
                command.Parameters.AddWithValue("@Title", note.Title);
                command.Parameters.AddWithValue("@Content", note.Content);
            });
        }

        public bool DeleteNote(int id)
        {
            return Execute("DELETE FROM Notes WHERE Id = @Id",
                command => command.Parameters.AddWithValue("@Id", id));
        }

        public List<Reminder> GetAllReminders()
        {
            return Query("SELECT * FROM Reminders", null, ReadReminder);
        }

        public bool CreateReminder(Reminder reminder)
        {
            return Execute("INSERT INTO Reminders (UserId, TaskId, ReminderTime, Message, IsActive) " +
                           "VALUES (@UserId, @TaskId, @ReminderTime, @Message, @IsActive)", command =>
            {
                command.Parameters.AddWithValue("@UserId", reminder.UserId);
                command.Parameters.AddWithValue("@TaskId", reminder.TaskId);
                command.Parameters.AddWithValue("@ReminderTime", reminder.ReminderDate);
                command.Parameters.AddWithValue("@Message", reminder.Message);
                command.Parameters.AddWithValue("@IsActive", reminder.IsShown);
            }, true);
        }

        public bool UpdateReminder(Reminder reminder)
        {
            return Execute("UPDATE Reminders SET UserId = @UserId, TaskId = @TaskId, ReminderTime = @ReminderTime, " +
                           "Message = @Message, IsActive = @IsActive WHERE Id = @Id", command =>
            {
                command.Parameters.AddWithValue("@Id", reminder.Id);
                command.Parameters.AddWithValue("@UserId", reminder.UserId);
                command.Parameters.AddWithValue("@TaskId", reminder.TaskId);
                command.Parameters.AddWithValue("@ReminderTime", reminder.ReminderDate);
                command.Parameters.AddWithValue("@Message", reminder.Message);
                command.Parameters.AddWithValue("@IsActive", reminder.IsShown);
            });
        }

        private List<T> Query<T>(string query, Action<SqlCommand> setup, Func<SqlDataReader, T> read)
        {
            var items = new List<T>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        setup?.Invoke(command);
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                items.Add(read(reader));
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
            return items;
        }

        private bool Execute(string query, Action<SqlCommand> setup, bool showError = false)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        setup?.Invoke(command);
                        return command.ExecuteNonQuery() > 0;
                    }
                }
                catch (Exception ex)
                {
                    if (showError)
                    {
                        System.Windows.MessageBox.Show(GetFriendlyDatabaseError(ex), "Ошибка базы данных",
                            System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                    }
                    else
                    {
                        Console.WriteLine($"Error: {ex.Message}");
                    }
                    return false;
                }
            }
        }

        private object Scalar(string query, Action<SqlCommand> setup, bool showError = false)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        setup?.Invoke(command);
                        return command.ExecuteScalar();
                    }
                }
                catch (Exception ex)
                {
                    if (showError)
                    {
                        System.Windows.MessageBox.Show(GetFriendlyDatabaseError(ex), "Ошибка базы данных",
                            System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                    }
                    else
                    {
                        Console.WriteLine($"Error: {ex.Message}");
                    }
                    return null;
                }
            }
        }

        private static User ReadUser(SqlDataReader reader)
        {
            return new User
            {
                Id = Convert.ToInt32(reader["Id"]),
                Username = reader["Username"].ToString(),
                PasswordHash = reader["PasswordHash"].ToString(),
                Email = reader["Email"] != DBNull.Value ? reader["Email"].ToString() : string.Empty,
                GenderId = Convert.ToInt32(reader["GenderId"]),
                RoleId = Convert.ToInt32(reader["RoleId"]),
                CreatedAt = reader["CreatedAt"] != DBNull.Value ? Convert.ToDateTime(reader["CreatedAt"]) : DateTime.Now
            };
        }

        private static Models.Task ReadTask(SqlDataReader reader)
        {
            return new Models.Task
            {
                Id = Convert.ToInt32(reader["Id"]),
                UserId = Convert.ToInt32(reader["UserId"]),
                Title = reader["Title"].ToString(),
                Description = reader["Description"] != DBNull.Value ? reader["Description"].ToString() : string.Empty,
                DueDate = Convert.ToDateTime(reader["DueDate"]),
                Status = reader["Status"] != DBNull.Value ? reader["Status"].ToString() : "Ожидает",
                Priority = reader["Priority"] != DBNull.Value ? reader["Priority"].ToString() : "Средний"
            };
        }

        private static Event ReadEvent(SqlDataReader reader)
        {
            return new Event
            {
                Id = Convert.ToInt32(reader["Id"]),
                UserId = Convert.ToInt32(reader["UserId"]),
                Title = reader["Title"].ToString(),
                Description = reader["Description"] != DBNull.Value ? reader["Description"].ToString() : string.Empty,
                StartDate = Convert.ToDateTime(reader["StartDate"]),
                EndDate = Convert.ToDateTime(reader["EndDate"]),
                Location = reader["Location"] != DBNull.Value ? reader["Location"].ToString() : string.Empty,
                Status = reader["Status"] != DBNull.Value ? reader["Status"].ToString() : "Запланировано"
            };
        }

        private static Note ReadNote(SqlDataReader reader)
        {
            return new Note
            {
                Id = Convert.ToInt32(reader["Id"]),
                UserId = Convert.ToInt32(reader["UserId"]),
                Title = reader["Title"].ToString(),
                Content = reader["Content"].ToString(),
                CreatedDate = Convert.ToDateTime(reader["CreatedAt"])
            };
        }

        private static Reminder ReadReminder(SqlDataReader reader)
        {
            return new Reminder
            {
                Id = Convert.ToInt32(reader["Id"]),
                UserId = Convert.ToInt32(reader["UserId"]),
                TaskId = reader["TaskId"] == DBNull.Value ? 0 : Convert.ToInt32(reader["TaskId"]),
                ReminderDate = Convert.ToDateTime(reader["ReminderTime"]),
                Message = reader["Message"] == DBNull.Value ? string.Empty : reader["Message"].ToString(),
                IsShown = reader["IsActive"] == DBNull.Value || Convert.ToBoolean(reader["IsActive"])
            };
        }
    }
}
