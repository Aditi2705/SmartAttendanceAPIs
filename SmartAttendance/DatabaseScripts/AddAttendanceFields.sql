-- Add CourseName, Semester and Year to Attendances table
-- Adjust schema/table name if your DB differs (default is dbo.Attendances)
ALTER TABLE dbo.Attendances
ADD CourseName NVARCHAR(MAX) NULL,
    Semester INT NOT NULL CONSTRAINT DF_Attendances_Semester DEFAULT 0,
    Year INT NOT NULL CONSTRAINT DF_Attendances_Year DEFAULT 0;

-- Quick verification (run after ALTER):
-- SELECT TOP 5 CourseName, Semester, Year FROM dbo.Attendances;
