-- DailyPlannerDB: сверка схемы Tasks с приложением
-- Приложение хранит приоритет строкой: 'Низкий' | 'Средний' | 'Высокий'
-- Приложение хранит статус строкой: 'Ожидает' | 'Выполнена'

USE DailyPlannerDB;
GO

-- ============================================================
-- 1. ПРИОРИТЕТ
-- ============================================================

-- 1.1. Исправить существующие числовые значения (после старых версий приложения)
UPDATE Tasks
SET Priority = CASE Priority
    WHEN '1' THEN 'Низкий'
    WHEN '2' THEN 'Средний'
    WHEN '3' THEN 'Высокий'
    ELSE Priority
END
WHERE Priority IN ('1', '2', '3');
GO

-- 1.2. Пересоздать CHECK-констрейнт под строковые значения
IF EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CHK_Tasks_Priority')
BEGIN
    ALTER TABLE dbo.Tasks DROP CONSTRAINT CHK_Tasks_Priority;
END
GO

ALTER TABLE dbo.Tasks
ADD CONSTRAINT CHK_Tasks_Priority CHECK (Priority IN (N'Низкий', N'Средний', N'Высокий'));
GO

-- ============================================================
-- 2. СТАТУС
-- Приложение пишет: N'Ожидает' (новая) и N'Выполнена' (выполненная).
-- ============================================================

-- 2.1. Привести существующие статусы к значениям приложения,
--      сохранив признак выполненной задачи.
UPDATE Tasks
SET Status = CASE
    WHEN Status LIKE N'%ыполнен%' OR Status LIKE N'Completed%' OR Status LIKE N'Done%' THEN N'Выполнена'
    ELSE N'Ожидает'
END
WHERE Status IS NULL OR Status NOT IN (N'Ожидает', N'Выполнена');
GO

-- 2.2. Пересоздать CHECK-констрейнт под значения приложения
IF EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CHK_Tasks_Status')
BEGIN
    ALTER TABLE dbo.Tasks DROP CONSTRAINT CHK_Tasks_Status;
END
GO

ALTER TABLE dbo.Tasks
ADD CONSTRAINT CHK_Tasks_Status CHECK (Status IN (N'Ожидает', N'Выполнена'));
GO

-- ============================================================
-- 3. Проверка
-- ============================================================
SELECT name, definition FROM sys.check_constraints
WHERE name IN ('CHK_Tasks_Priority', 'CHK_Tasks_Status');
GO
