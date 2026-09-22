using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using DailyPlaner.Models;

namespace DailyPlaner.Services
{
    public class DatabaseService
    {
        private readonly string connectionString;

        public DatabaseService()
        {
            connectionString = GetConnectionString();
        }

        public DatabaseService(string customConnectionString)
        {
            connectionString = customConnectionString;
        }

        private static string GetConnectionString()
        {
            string configured = System.Configuration.ConfigurationManager.ConnectionStrings["DailyPlannerConnection"]?.ConnectionString;
            if (!string.IsNullOrWhiteSpace(configured))
            {
                return configured;
            }
            return "Server=PCGl1tch;Database=DailyPlannerDB;Trusted_Connection=True;TrustServerCertificate=True;";
        }

        public static string GetFriendlyDatabaseError(SqlException ex)
        {
            switch (ex.Number)
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
                    return $"Ошибка базы данных (код {ex.Number}): {ex.Message}";
            }
        }

        public bool TestConnection()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    return true;
                }
                catch (SqlException ex)
                {
                    throw new InvalidOperationException(GetFriendlyDatabaseError(ex), ex);
                }
            }
        }

        #region User Methods

        public List<User> GetAllUsers()
        {
            var users = new List<User>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "SELECT * FROM Users";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var user = new User
                                {
                                    Id = reader["Id"] != DBNull.Value ? Convert.ToInt32(reader["Id"]) : 0,
                                    Username = reader["Username"] != DBNull.Value ? reader["Username"].ToString() : string.Empty,
                                    PasswordHash = reader["PasswordHash"] != DBNull.Value ? reader["PasswordHash"].ToString() : string.Empty,
                                    Email = reader["Email"] != DBNull.Value ? reader["Email"].ToString() : string.Empty,
                                    GenderId = reader["GenderId"] != DBNull.Value ? Convert.ToInt32(reader["GenderId"]) : 0,
                                    RoleId = reader["RoleId"] != DBNull.Value ? Convert.ToInt32(reader["RoleId"]) : 0,
                                    CreatedAt = reader["CreatedAt"] != DBNull.Value ? Convert.ToDateTime(reader["CreatedAt"]) : DateTime.Now
                                };
                                users.Add(user);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
            return users;
        }

        public User GetUserById(int id)
        {
            User user = null;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "SELECT * FROM Users WHERE Id = @Id";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Id", id);
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                user = new User
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
                        }
                    }
                }
                catch (SqlException ex)
                {
                    Console.WriteLine($"Database error getting user: {ex.Message}");
                    throw new InvalidOperationException(GetFriendlyDatabaseError(ex), ex);
                }
            }
            return user;
        }

        public User GetUserByUsername(string username)
        {
            User user = null;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "SELECT * FROM Users WHERE Username = @Username";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Username", username);
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                user = new User
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
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
            return user;
        }

        public bool CreateUser(User user)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "INSERT INTO Users (Username, PasswordHash, Email, GenderId, RoleId, CreatedAt) " +
                                   "VALUES (@Username, @PasswordHash, @Email, @GenderId, @RoleId, @CreatedAt)";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Username", user.Username);
                        command.Parameters.AddWithValue("@PasswordHash", user.PasswordHash);
                        command.Parameters.AddWithValue("@Email", user.Email ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@GenderId", user.GenderId);
                        command.Parameters.AddWithValue("@RoleId", user.RoleId);
                        command.Parameters.AddWithValue("@CreatedAt", user.CreatedAt);
                        int result = command.ExecuteNonQuery();
                        return result > 0;
                    }
                }
                catch (SqlException ex)
                {
                    Console.WriteLine($"Database error creating user: {ex.Message}");
                    throw new InvalidOperationException(GetFriendlyDatabaseError(ex), ex);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    throw;
                }
            }
        }

        public bool UsernameExists(string username)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "SELECT COUNT(1) FROM Users WHERE Username = @Username";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Username", username);
                        return Convert.ToInt32(command.ExecuteScalar()) > 0;
                    }
                }
                catch (SqlException ex)
                {
                    Console.WriteLine($"Database error checking username: {ex.Message}");
                    throw new InvalidOperationException(GetFriendlyDatabaseError(ex), ex);
                }
            }
        }

        public static string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                return string.Empty;
            }
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                byte[] bytes = System.Text.Encoding.UTF8.GetBytes(password);
                byte[] hash = sha256.ComputeHash(bytes);
                var stringBuilder = new System.Text.StringBuilder(hash.Length * 2);
                foreach (byte b in hash)
                {
                    stringBuilder.Append(b.ToString("x2"));
                }
                return stringBuilder.ToString();
            }
        }

        public bool UpdateUser(User user)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "UPDATE Users SET Username = @Username, PasswordHash = @PasswordHash, Email = @Email, " +
                                   "GenderId = @GenderId, RoleId = @RoleId " +
                                   "WHERE Id = @Id";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Id", user.Id);
                        command.Parameters.AddWithValue("@Username", user.Username);
                        command.Parameters.AddWithValue("@PasswordHash", user.PasswordHash);
                        command.Parameters.AddWithValue("@Email", user.Email ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@GenderId", user.GenderId);
                        command.Parameters.AddWithValue("@RoleId", user.RoleId);
                        int result = command.ExecuteNonQuery();
                        return result > 0;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    return false;
                }
            }
        }

        public bool DeleteUser(int id)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "DELETE FROM Users WHERE Id = @Id";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Id", id);
                        int result = command.ExecuteNonQuery();
                        return result > 0;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    return false;
                }
            }
        }
        #endregion

        #region Task Methods
        public List<Models.Task> GetAllTasks()
        {
            var tasks = new List<Models.Task>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "SELECT * FROM Tasks";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var task = new Models.Task
                                {
                                    Id = Convert.ToInt32(reader["Id"]),
                                    UserId = Convert.ToInt32(reader["UserId"]),
                                    Title = reader["Title"].ToString(),
                                    Description = reader["Description"] != DBNull.Value ? reader["Description"].ToString() : string.Empty,
                                    DueDate = Convert.ToDateTime(reader["DueDate"]),
                                    Status = reader["Status"] != DBNull.Value ? reader["Status"].ToString() : "Ожидает",
                                    Priority = int.TryParse(reader["Priority"]?.ToString(), out int taskPriority) ? taskPriority : 1
                                };
                                tasks.Add(task);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
            return tasks;
        }

        public List<Models.Task> GetTasksByUserId(int userId)
        {
            var tasks = new List<Models.Task>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "SELECT * FROM Tasks WHERE UserId = @UserId";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@UserId", userId);
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var task = new Models.Task
                                {
                                    Id = Convert.ToInt32(reader["Id"]),
                                    UserId = Convert.ToInt32(reader["UserId"]),
                                    Title = reader["Title"].ToString(),
                                    Description = reader["Description"] != DBNull.Value ? reader["Description"].ToString() : string.Empty,
                                    DueDate = Convert.ToDateTime(reader["DueDate"]),
                                    Status = reader["Status"] != DBNull.Value ? reader["Status"].ToString() : "Ожидает",
                                    Priority = int.TryParse(reader["Priority"]?.ToString(), out int taskPriority) ? taskPriority : 1
                                };
                                tasks.Add(task);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
            return tasks;
        }

        public Models.Task GetTaskById(int id)
        {
            Models.Task task = null;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "SELECT * FROM Tasks WHERE Id = @Id";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Id", id);
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                task = new Models.Task
                                {
                                    Id = Convert.ToInt32(reader["Id"]),
                                    UserId = Convert.ToInt32(reader["UserId"]),
                                    Title = reader["Title"].ToString(),
                                    Description = reader["Description"] != DBNull.Value ? reader["Description"].ToString() : string.Empty,
                                    DueDate = Convert.ToDateTime(reader["DueDate"]),
                                    Status = reader["Status"] != DBNull.Value ? reader["Status"].ToString() : "Ожидает",
                                    Priority = int.TryParse(reader["Priority"]?.ToString(), out int taskPriority) ? taskPriority : 1
                                };
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
            return task;
        }

        public bool CreateTask(Models.Task task)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "INSERT INTO Tasks (UserId, Title, Description, DueDate, Priority, Status) " +
                                   "VALUES (@UserId, @Title, @Description, @DueDate, @Priority, @Status)";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@UserId", task.UserId);
                        command.Parameters.AddWithValue("@Title", task.Title);
                        command.Parameters.AddWithValue("@Description", task.Description ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@DueDate", task.DueDate);
                        command.Parameters.AddWithValue("@Priority", task.Priority.ToString());
                        command.Parameters.AddWithValue("@Status", task.IsCompleted ? "Выполнена" : "Ожидает");
                        int result = command.ExecuteNonQuery();
                        return result > 0;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    return false;
                }
            }
        }

        public bool UpdateTask(Models.Task task)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "UPDATE Tasks SET UserId = @UserId, Title = @Title, Description = @Description, " +
                                   "DueDate = @DueDate, Priority = @Priority, Status = @Status " +
                                   "WHERE Id = @Id";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Id", task.Id);
                        command.Parameters.AddWithValue("@UserId", task.UserId);
                        command.Parameters.AddWithValue("@Title", task.Title);
                        command.Parameters.AddWithValue("@Description", task.Description ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@DueDate", task.DueDate);
                        command.Parameters.AddWithValue("@Priority", task.Priority.ToString());
                        command.Parameters.AddWithValue("@Status", task.IsCompleted ? "Выполнена" : "Ожидает");
                        int result = command.ExecuteNonQuery();
                        return result > 0;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    return false;
                }
            }
        }

        public bool DeleteTask(int id)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "DELETE FROM Tasks WHERE Id = @Id";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Id", id);
                        int result = command.ExecuteNonQuery();
                        return result > 0;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    return false;
                }
            }
        }
        #endregion

        #region Event Methods
        public List<Event> GetAllEvents()
        {
            var events = new List<Event>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "SELECT * FROM Events";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var ev = new Event
                                {
                                    Id = Convert.ToInt32(reader["Id"]),
                                    UserId = Convert.ToInt32(reader["UserId"]),
                                    Title = reader["Title"].ToString(),
                                    Description = reader["Description"] != DBNull.Value ? reader["Description"].ToString() : string.Empty,
                                    StartDate = Convert.ToDateTime(reader["StartDate"]),
                                    EndDate = Convert.ToDateTime(reader["EndDate"]),
                                    Location = reader["Location"] != DBNull.Value ? reader["Location"].ToString() : string.Empty
                                };
                                events.Add(ev);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
            return events;
        }

        public List<Event> GetEventsByUserId(int userId)
        {
            var events = new List<Event>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "SELECT * FROM Events WHERE UserId = @UserId";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@UserId", userId);
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var ev = new Event
                                {
                                    Id = Convert.ToInt32(reader["Id"]),
                                    UserId = Convert.ToInt32(reader["UserId"]),
                                    Title = reader["Title"].ToString(),
                                    Description = reader["Description"] != DBNull.Value ? reader["Description"].ToString() : string.Empty,
                                    StartDate = Convert.ToDateTime(reader["StartDate"]),
                                    EndDate = Convert.ToDateTime(reader["EndDate"]),
                                    Location = reader["Location"] != DBNull.Value ? reader["Location"].ToString() : string.Empty
                                };
                                events.Add(ev);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
            return events;
        }

        public Event GetEventById(int id)
        {
            Event ev = null;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "SELECT * FROM Events WHERE Id = @Id";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Id", id);
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                ev = new Event
                                {
                                    Id = Convert.ToInt32(reader["Id"]),
                                    UserId = Convert.ToInt32(reader["UserId"]),
                                    Title = reader["Title"].ToString(),
                                    Description = reader["Description"] != DBNull.Value ? reader["Description"].ToString() : string.Empty,
                                    StartDate = Convert.ToDateTime(reader["StartDate"]),
                                    EndDate = Convert.ToDateTime(reader["EndDate"]),
                                    Location = reader["Location"] != DBNull.Value ? reader["Location"].ToString() : string.Empty
                                };
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
            return ev;
        }

        public bool CreateEvent(Event ev)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "INSERT INTO Events (UserId, Title, Description, StartDate, EndDate, Location) " +
                                   "VALUES (@UserId, @Title, @Description, @StartDate, @EndDate, @Location)";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@UserId", ev.UserId);
                        command.Parameters.AddWithValue("@Title", ev.Title);
                        command.Parameters.AddWithValue("@Description", ev.Description ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@StartDate", ev.StartDate);
                        command.Parameters.AddWithValue("@EndDate", ev.EndDate);
                        command.Parameters.AddWithValue("@Location", ev.Location ?? (object)DBNull.Value);
                        int result = command.ExecuteNonQuery();
                        return result > 0;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    return false;
                }
            }
        }

        public bool UpdateEvent(Event ev)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "UPDATE Events SET UserId = @UserId, Title = @Title, Description = @Description, " +
                                   "StartDate = @StartDate, EndDate = @EndDate, Location = @Location " +
                                   "WHERE Id = @Id";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Id", ev.Id);
                        command.Parameters.AddWithValue("@UserId", ev.UserId);
                        command.Parameters.AddWithValue("@Title", ev.Title);
                        command.Parameters.AddWithValue("@Description", ev.Description ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@StartDate", ev.StartDate);
                        command.Parameters.AddWithValue("@EndDate", ev.EndDate);
                        command.Parameters.AddWithValue("@Location", ev.Location ?? (object)DBNull.Value);
                        int result = command.ExecuteNonQuery();
                        return result > 0;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    return false;
                }
            }
        }

        public bool DeleteEvent(int id)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "DELETE FROM Events WHERE Id = @Id";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Id", id);
                        int result = command.ExecuteNonQuery();
                        return result > 0;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    return false;
                }
            }
        }
        #endregion

        #region Note Methods
        public List<Note> GetAllNotes()
        {
            var notes = new List<Note>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "SELECT * FROM Notes";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var note = new Note
                                {
                                    Id = Convert.ToInt32(reader["Id"]),
                                    UserId = Convert.ToInt32(reader["UserId"]),
                                    Title = reader["Title"].ToString(),
                                    Content = reader["Content"].ToString(),
                                    CreatedDate = Convert.ToDateTime(reader["CreatedAt"])
                                };
                                notes.Add(note);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
            return notes;
        }

        public List<Note> GetNotesByUserId(int userId)
        {
            var notes = new List<Note>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "SELECT * FROM Notes WHERE UserId = @UserId";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@UserId", userId);
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var note = new Note
                                {
                                    Id = Convert.ToInt32(reader["Id"]),
                                    UserId = Convert.ToInt32(reader["UserId"]),
                                    Title = reader["Title"].ToString(),
                                    Content = reader["Content"].ToString(),
                                    CreatedDate = Convert.ToDateTime(reader["CreatedAt"])
                                };
                                notes.Add(note);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
            return notes;
        }

        public Note GetNoteById(int id)
        {
            Note note = null;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "SELECT * FROM Notes WHERE Id = @Id";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Id", id);
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                note = new Note
                                {
                                    Id = Convert.ToInt32(reader["Id"]),
                                    UserId = Convert.ToInt32(reader["UserId"]),
                                    Title = reader["Title"].ToString(),
                                    Content = reader["Content"].ToString(),
                                    CreatedDate = Convert.ToDateTime(reader["CreatedAt"])
                                };
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
            return note;
        }

        public bool CreateNote(Note note)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "INSERT INTO Notes (UserId, Title, Content, CreatedAt) " +
                                   "VALUES (@UserId, @Title, @Content, @CreatedAt)";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@UserId", note.UserId);
                        command.Parameters.AddWithValue("@Title", note.Title);
                        command.Parameters.AddWithValue("@Content", note.Content);
                        command.Parameters.AddWithValue("@CreatedAt", note.CreatedDate);
                        int result = command.ExecuteNonQuery();
                        return result > 0;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    return false;
                }
            }
        }

        public bool UpdateNote(Note note)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "UPDATE Notes SET UserId = @UserId, Title = @Title, Content = @Content " +
                                   "WHERE Id = @Id";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Id", note.Id);
                        command.Parameters.AddWithValue("@UserId", note.UserId);
                        command.Parameters.AddWithValue("@Title", note.Title);
                        command.Parameters.AddWithValue("@Content", note.Content);
                        int result = command.ExecuteNonQuery();
                        return result > 0;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    return false;
                }
            }
        }

        public bool DeleteNote(int id)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "DELETE FROM Notes WHERE Id = @Id";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Id", id);
                        int result = command.ExecuteNonQuery();
                        return result > 0;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    return false;
                }
            }
        }
        #endregion

        #region Tag Methods
        public List<Tag> GetAllTags()
        {
            var tags = new List<Tag>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "SELECT * FROM Tags";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var tag = new Tag
                                {
                                    Id = Convert.ToInt32(reader["Id"]),
                                    Name = reader["Name"].ToString(),
                                    Color = reader["Color"] != DBNull.Value ? reader["Color"].ToString() : string.Empty
                                };
                                tags.Add(tag);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
            return tags;
        }

        public Tag GetTagById(int id)
        {
            Tag tag = null;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "SELECT * FROM Tags WHERE Id = @Id";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Id", id);
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                tag = new Tag
                                {
                                    Id = Convert.ToInt32(reader["Id"]),
                                    Name = reader["Name"].ToString(),
                                    Color = reader["Color"] != DBNull.Value ? reader["Color"].ToString() : string.Empty
                                };
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
            return tag;
        }

        public bool CreateTag(Tag tag)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "INSERT INTO Tags (Name, Color) VALUES (@Name, @Color)";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Name", tag.Name);
                        command.Parameters.AddWithValue("@Color", tag.Color ?? (object)DBNull.Value);
                        int result = command.ExecuteNonQuery();
                        return result > 0;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    return false;
                }
            }
        }

        public bool UpdateTag(Tag tag)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "UPDATE Tags SET Name = @Name, Color = @Color WHERE Id = @Id";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Id", tag.Id);
                        command.Parameters.AddWithValue("@Name", tag.Name);
                        command.Parameters.AddWithValue("@Color", tag.Color ?? (object)DBNull.Value);
                        int result = command.ExecuteNonQuery();
                        return result > 0;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    return false;
                }
            }
        }

        public bool DeleteTag(int id)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "DELETE FROM Tags WHERE Id = @Id";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Id", id);
                        int result = command.ExecuteNonQuery();
                        return result > 0;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    return false;
                }
            }
        }
        #endregion

        #region Reminder Methods
        public List<Reminder> GetAllReminders()
        {
            var reminders = new List<Reminder>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "SELECT * FROM Reminders";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var reminder = new Reminder
                                {
                                    Id = Convert.ToInt32(reader["Id"]),
                                    UserId = Convert.ToInt32(reader["UserId"]),
                                    TaskId = Convert.ToInt32(reader["TaskId"]),
                                    ReminderDate = Convert.ToDateTime(reader["ReminderTime"]),
                                    Message = reader["Message"].ToString(),
                                    IsShown = Convert.ToBoolean(reader["IsActive"])
                                };
                                reminders.Add(reminder);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
            return reminders;
        }

        public List<Reminder> GetRemindersByUserId(int userId)
        {
            var reminders = new List<Reminder>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "SELECT * FROM Reminders WHERE UserId = @UserId";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@UserId", userId);
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var reminder = new Reminder
                                {
                                    Id = Convert.ToInt32(reader["Id"]),
                                    UserId = Convert.ToInt32(reader["UserId"]),
                                    TaskId = Convert.ToInt32(reader["TaskId"]),
                                    ReminderDate = Convert.ToDateTime(reader["ReminderTime"]),
                                    Message = reader["Message"].ToString(),
                                    IsShown = Convert.ToBoolean(reader["IsActive"])
                                };
                                reminders.Add(reminder);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
            return reminders;
        }

        public bool CreateReminder(Reminder reminder)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "INSERT INTO Reminders (UserId, TaskId, ReminderTime, Message, IsActive) " +
                                   "VALUES (@UserId, @TaskId, @ReminderTime, @Message, @IsActive)";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@UserId", reminder.UserId);
                        command.Parameters.AddWithValue("@TaskId", reminder.TaskId);
                        command.Parameters.AddWithValue("@ReminderTime", reminder.ReminderDate);
                        command.Parameters.AddWithValue("@Message", reminder.Message);
                        command.Parameters.AddWithValue("@IsActive", reminder.IsShown);
                        int result = command.ExecuteNonQuery();
                        return result > 0;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    return false;
                }
            }
        }

        public bool UpdateReminder(Reminder reminder)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "UPDATE Reminders SET UserId = @UserId, TaskId = @TaskId, ReminderTime = @ReminderTime, " +
                                   "Message = @Message, IsActive = @IsActive WHERE Id = @Id";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Id", reminder.Id);
                        command.Parameters.AddWithValue("@UserId", reminder.UserId);
                        command.Parameters.AddWithValue("@TaskId", reminder.TaskId);
                        command.Parameters.AddWithValue("@ReminderTime", reminder.ReminderDate);
                        command.Parameters.AddWithValue("@Message", reminder.Message);
                        command.Parameters.AddWithValue("@IsActive", reminder.IsShown);
                        int result = command.ExecuteNonQuery();
                        return result > 0;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    return false;
                }
            }
        }

        public bool DeleteReminder(int id)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "DELETE FROM Reminders WHERE Id = @Id";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Id", id);
                        int result = command.ExecuteNonQuery();
                        return result > 0;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    return false;
                }
            }
        }
        #endregion

        #region Gender Methods
        public List<Gender> GetAllGenders()
        {
            var genders = new List<Gender>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "SELECT * FROM Genders";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var gender = new Gender
                                {
                                    Id = Convert.ToInt32(reader["Id"]),
                                    Name = reader["Name"].ToString()
                                };
                                genders.Add(gender);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
            return genders;
        }
        #endregion

        #region Role Methods
        public List<Role> GetAllRoles()
        {
            var roles = new List<Role>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "SELECT * FROM Roles";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var role = new Role
                                {
                                    Id = Convert.ToInt32(reader["Id"]),
                                    Name = reader["Name"].ToString()
                                };
                                roles.Add(role);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
            return roles;
        }
        #endregion
    }
}
