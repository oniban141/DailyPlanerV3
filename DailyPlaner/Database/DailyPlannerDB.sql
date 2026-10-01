-- ============================================================
-- DailyPlannerDB — скрипт создания базы данных «Ежедневник»
-- Сервер: локальный SQL Server, Windows Authentication
-- Имена таблиц и колонок соответствуют коду приложения
-- (DatabaseService: Users, Genders, Tasks, Events, Notes, Reminders)
-- ============================================================

USE master;
GO

IF EXISTS (SELECT 1 FROM sys.databases WHERE name = N'DailyPlannerDB')
BEGIN
    DROP DATABASE DailyPlannerDB;
END
GO

CREATE DATABASE DailyPlannerDB;
GO

USE DailyPlannerDB;
GO

-- ============================================================
-- СПРАВОЧНИКИ
-- ============================================================

CREATE TABLE Genders (
    Id INT PRIMARY KEY,
    Name NVARCHAR(50) NOT NULL
);
GO

CREATE TABLE Roles (
    Id INT PRIMARY KEY,
    Name NVARCHAR(50) NOT NULL
);
GO

-- ============================================================
-- ПОЛЬЗОВАТЕЛИ
-- ============================================================

CREATE TABLE Users (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Username NVARCHAR(50) NOT NULL,
    PasswordHash VARCHAR(64) NOT NULL,
    Email NVARCHAR(100) NULL,
    GenderId INT NULL,
    RoleId INT NOT NULL DEFAULT 1,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (GenderId) REFERENCES Genders(Id),
    FOREIGN KEY (RoleId) REFERENCES Roles(Id)
);
GO

-- ============================================================
-- ЗАДАЧИ
-- Приоритет: N'Низкий' | N'Средний' | N'Высокий'
-- Статус: N'Ожидает' | N'Выполнена'
-- ============================================================

CREATE TABLE Tasks (
    Id INT PRIMARY KEY IDENTITY(1,1),
    UserId INT NOT NULL,
    Title NVARCHAR(200) NOT NULL,
    Description NVARCHAR(1000) NULL,
    DueDate DATETIME NOT NULL,
    Priority NVARCHAR(20) NOT NULL DEFAULT N'Средний',
    Status NVARCHAR(50) NOT NULL DEFAULT N'Ожидает',
    FOREIGN KEY (UserId) REFERENCES Users(Id),
    CONSTRAINT CHK_Tasks_Priority CHECK (Priority IN (N'Низкий', N'Средний', N'Высокий')),
    CONSTRAINT CHK_Tasks_Status CHECK (Status IN (N'Ожидает', N'Выполнена'))
);
GO

-- ============================================================
-- СОБЫТИЯ
-- Статус: N'Запланировано' | N'Завершено'
-- ============================================================

CREATE TABLE Events (
    Id INT PRIMARY KEY IDENTITY(1,1),
    UserId INT NOT NULL,
    Title NVARCHAR(200) NOT NULL,
    Description NVARCHAR(1000) NULL,
    StartDate DATETIME NOT NULL,
    EndDate DATETIME NOT NULL,
    Location NVARCHAR(200) NULL,
    Status NVARCHAR(50) NOT NULL DEFAULT N'Запланировано',
    FOREIGN KEY (UserId) REFERENCES Users(Id),
    CONSTRAINT CHK_Events_Status CHECK (Status IN (N'Запланировано', N'Завершено')),
    CONSTRAINT CHK_Events_Dates CHECK (EndDate >= StartDate)
);
GO

-- ============================================================
-- ЗАМЕТКИ
-- ============================================================

CREATE TABLE Notes (
    Id INT PRIMARY KEY IDENTITY(1,1),
    UserId INT NOT NULL,
    Title NVARCHAR(200) NOT NULL,
    Content NVARCHAR(2000) NULL,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (UserId) REFERENCES Users(Id)
);
GO

-- ============================================================
-- НАПОМИНАНИЯ
-- Напоминание привязано либо к задаче, либо к событию
-- ============================================================

CREATE TABLE Reminders (
    Id INT PRIMARY KEY IDENTITY(1,1),
    UserId INT NOT NULL,
    TaskId INT NULL,
    EventId INT NULL,
    ReminderTime DATETIME NOT NULL,
    Message NVARCHAR(500) NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    FOREIGN KEY (UserId) REFERENCES Users(Id),
    FOREIGN KEY (TaskId) REFERENCES Tasks(Id),
    FOREIGN KEY (EventId) REFERENCES Events(Id),
    CONSTRAINT CHK_Reminders_Source CHECK (
        (TaskId IS NOT NULL AND EventId IS NULL) OR
        (TaskId IS NULL AND EventId IS NOT NULL))
);
GO

-- ============================================================
-- НАПОЛНЕНИЕ СПРАВОЧНИКОВ
-- ============================================================

INSERT INTO Genders (Id, Name) VALUES
    (1, N'Мужской'),
    (2, N'Женский');
GO

INSERT INTO Roles (Id, Name) VALUES
    (1, N'Пользователь'),
    (2, N'Администратор');
GO

-- ============================================================
-- ТЕСТОВЫЙ ПОЛЬЗОВАТЕЛЬ: admin / admin
-- Пароль хранится как SHA-256 хеш (приложение хеширует при входе/регистрации)
-- SHA-256 от 'admin':
-- 03ac674216f3e15c761ee1a5e255f067953623c8b388b4459e13f978d7c846f4
-- ============================================================

INSERT INTO Users (Username, PasswordHash, Email, GenderId, RoleId)
VALUES (
    N'admin',
    '03ac674216f3e15c761ee1a5e255f067953623c8b388b4459e13f978d7c846f4',
    N'admin@example.com',
    1,
    2);
GO

DECLARE @AdminUserId INT = (SELECT TOP 1 Id FROM Users WHERE Username = N'admin');

-- ============================================================
-- ТЕСТОВЫЕ ДАННЫЕ
-- ============================================================

INSERT INTO Tasks (UserId, Title, Description, DueDate, Priority, Status)
VALUES
    (@AdminUserId, N'Спланировать день', N'Заполнить план на неделю', DATEADD(HOUR, 17, CAST(GETDATE() AS DATE)), N'Средний', N'Ожидает'),
    (@AdminUserId, N'Купить продукты', N'Хлеб, молоко, овощи', DATEADD(DAY, 1, DATEADD(HOUR, 12, CAST(GETDATE() AS DATE))), N'Низкий', N'Ожидает'),
    (@AdminUserId, N'Сдать курсовую', N'Подготовить и отправить работу', DATEADD(DAY, 3, DATEADD(HOUR, 10, CAST(GETDATE() AS DATE))), N'Высокий', N'Ожидает');
GO

INSERT INTO Events (UserId, Title, Description, StartDate, EndDate, Location, Status)
VALUES
    (@AdminUserId, N'Собрание', N'Еженедельная планёрка', DATEADD(HOUR, 5, CAST(GETDATE() AS DATE)), DATEADD(HOUR, 6, CAST(GETDATE() AS DATE)), N'Офис, переговорная', N'Запланировано'),
    (@AdminUserId, N'Тренировка', N'Бассейн', DATEADD(DAY, 1, DATEADD(HOUR, 7, CAST(GETDATE() AS DATE))), DATEADD(DAY, 1, DATEADD(HOUR, 8, CAST(GETDATE() AS DATE))), N'Спортзал', N'Запланировано');
GO

INSERT INTO Notes (UserId, Title, Content)
VALUES
    (@AdminUserId, N'Идеи для проекта', N'Записывать все идеи в заметки, чтобы не забыть');
GO

INSERT INTO Reminders (UserId, TaskId, ReminderTime, Message, IsActive)
VALUES
    (@AdminUserId, (SELECT TOP 1 Id FROM Tasks WHERE Title = N'Спланировать день'), DATEADD(HOUR, 16, CAST(GETDATE() AS DATE)), N'Скоро: Спланировать день', 1);
GO
