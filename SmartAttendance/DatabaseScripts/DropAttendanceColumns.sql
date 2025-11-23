USE [AttendanceDb];
GO
SET NOCOUNT ON;

-- Drop default constraint on Semester if present
DECLARE @c NVARCHAR(200);
SELECT @c = dc.name
FROM sys.default_constraints dc
JOIN sys.columns c ON dc.parent_object_id = c.object_id AND dc.parent_column_id = c.column_id
JOIN sys.tables t ON t.object_id = c.object_id
WHERE t.name = 'Attendances' AND c.name = 'Semester';
IF @c IS NOT NULL
BEGIN
    EXEC('ALTER TABLE dbo.Attendances DROP CONSTRAINT [' + @c + ']');
    PRINT 'Dropped constraint ' + @c;
END

-- Drop default constraint on Year if present
SELECT @c = dc.name
FROM sys.default_constraints dc
JOIN sys.columns c ON dc.parent_object_id = c.object_id AND dc.parent_column_id = c.column_id
JOIN sys.tables t ON t.object_id = c.object_id
WHERE t.name = 'Attendances' AND c.name = 'Year';
IF @c IS NOT NULL
BEGIN
    EXEC('ALTER TABLE dbo.Attendances DROP CONSTRAINT [' + @c + ']');
    PRINT 'Dropped constraint ' + @c;
END

-- Now drop columns if they exist
IF EXISTS(SELECT 1 FROM sys.columns WHERE Name='CourseName' AND Object_ID = OBJECT_ID('dbo.Attendances'))
BEGIN
    ALTER TABLE dbo.Attendances DROP COLUMN CourseName;
    PRINT 'Dropped CourseName';
END

IF EXISTS(SELECT 1 FROM sys.columns WHERE Name='Semester' AND Object_ID = OBJECT_ID('dbo.Attendances'))
BEGIN
    ALTER TABLE dbo.Attendances DROP COLUMN Semester;
    PRINT 'Dropped Semester';
END

IF EXISTS(SELECT 1 FROM sys.columns WHERE Name='Year' AND Object_ID = OBJECT_ID('dbo.Attendances'))
BEGIN
    ALTER TABLE dbo.Attendances DROP COLUMN Year;
    PRINT 'Dropped Year';
END
