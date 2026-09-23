-- DailyPlannerDB: сверка схемы Tasks с приложением
-- Приложение хранит приоритет строкой: 'Низкий' | 'Средний' | 'Высокий'
-- Приложение хранит статус строкой: 'Ожидает' | 'Выполнена'

USE DailyPlannerDB;
GO

-- ============================================================
-- 1. ПРИОРИТЕТ
-- ============================================================

-- 1.1. Сначала удалить старый констрейнт (UPDATE ниже может нарушать его)
IF EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CHK_Tasks_Priority')
BEGIN
    ALTER TABLE dbo.Tasks DROP CONSTRAINT CHK_Tasks_Priority;
END
GO

-- 1.2. Исправить существующие числовые значения (после старых версий приложения)
UPDATE Tasks
SET Priority = CASE Priority
    WHEN '1' THEN 'Низкий'
    WHEN '2' THEN 'Средний'
    WHEN '3' THEN 'Высокий'
    ELSE Priority
END
WHERE Priority IN ('1', '2', '3');
GO

-- 1.3. Создать CHECK-констрейнт под строковые значения
ALTER TABLE dbo.Tasks
ADD CONSTRAINT CHK_Tasks_Priority CHECK (Priority IN (N'Низкий', N'Средний', N'Высокий'));
GO

-- ============================================================
-- 2. СТАТУС
-- Приложение пишет: N'Ожидает' (новая) и N'Выполнена' (выполненная).
-- ============================================================

-- 2.1. Сначала удалить старый констрейнт (UPDATE ниже может нарушать его)
IF EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CHK_Tasks_Status')
BEGIN
    ALTER TABLE dbo.Tasks DROP CONSTRAINT CHK_Tasks_Status;
END
GO

-- 2.2. Привести существующие статусы к значениям приложения,
--      сохранив признак выполненной задачи.
UPDATE Tasks
SET Status = CASE
    WHEN Status LIKE N'%ыполнен%' OR Status LIKE N'Completed%' OR Status LIKE N'Done%' OR Status LIKE N'Готово%' THEN N'Выполнена'
    ELSE N'Ожидает'
END
WHERE Status IS NULL OR Status NOT IN (N'Ожидает', N'Выполнена');
GO

-- 2.3. Создать CHECK-констрейнт под значения приложения
ALTER TABLE dbo.Tasks
ADD CONSTRAINT CHK_Tasks_Status CHECK (Status IN (N'Ожидает', N'Выполнена'));
GO

-- ============================================================
-- 3. Проверка
-- ============================================================
SELECT name, definition FROM sys.check_constraints
WHERE name IN ('CHK_Tasks_Priority', 'CHK_Tasks_Status');
GO
