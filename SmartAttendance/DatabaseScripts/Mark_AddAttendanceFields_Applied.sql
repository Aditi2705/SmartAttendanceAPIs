-- Mark EF migration 20251119184932_AddAttendanceFields as applied
-- Run this in SSMS against the AttendanceDb after you've manually added the columns (or after a partially failed migration)

USE [AttendanceDb];
GO

IF NOT EXISTS(SELECT 1 FROM dbo.__EFMigrationsHistory WHERE MigrationId = '20251119184932_AddAttendanceFields')
BEGIN
    INSERT INTO dbo.__EFMigrationsHistory (MigrationId, ProductVersion)
    VALUES ('20251119184932_AddAttendanceFields', '9.0.9');
END
GO

-- Verify:
-- SELECT MigrationId, ProductVersion FROM dbo.__EFMigrationsHistory ORDER BY MigrationId DESC;
