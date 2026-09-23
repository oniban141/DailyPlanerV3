-- DailyPlannerDB: сверка схемы Tasks.Priority с приложением
-- Приложение хранит приоритет строкой: 'Низкий' | 'Средний' | 'Высокий'

USE DailyPlannerDB;
GO

-- 1. Исправить существующие числовые значения (после старых версий приложения)
UPDATE Tasks
SET Priority = CASE Priority
    WHEN '1' THEN 'Низкий'
    WHEN '2' THEN 'Средний'
    WHEN '3' THEN 'Высокий'
    ELSE Priority
END
WHERE Priority IN ('1', '2', '3');
GO

-- 2. Пересоздать CHECK-констрейнт под строковые значения
IF EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CHK_Tasks_Priority')
BEGIN
    ALTER TABLE dbo.Tasks DROP CONSTRAINT CHK_Tasks_Priority;
END
GO

ALTER TABLE dbo.Tasks
ADD CONSTRAINT CHK_Tasks_Priority CHECK (Priority IN (N'Низкий', N'Средний', N'Высокий'));
GO

-- 3. Проверка
SELECT * FROM sys.check_constraints WHERE name = 'CHK_Tasks_Priority';
GO
