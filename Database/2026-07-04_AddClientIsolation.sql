USE HospitalDB;
GO

SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF NOT EXISTS (SELECT 1 FROM dbo.ConfigureClients)
BEGIN
    INSERT dbo.ConfigureClients (ProjectTitle, ClientName, IsActive)
    VALUES ('Equipment Maintenance & Reporting System', 'Hospital CMMS', 1);
END;

DECLARE @DefaultClientId int = (SELECT TOP (1) Id FROM dbo.ConfigureClients ORDER BY Id);

-- Add nullable columns first so existing installations can be backfilled safely.
IF COL_LENGTH('dbo.Users', 'ClientId') IS NULL ALTER TABLE dbo.Users ADD ClientId int NULL;
IF COL_LENGTH('dbo.DropdownMasters', 'ClientId') IS NULL ALTER TABLE dbo.DropdownMasters ADD ClientId int NULL;
IF COL_LENGTH('dbo.LocationMachineMasters', 'ClientId') IS NULL ALTER TABLE dbo.LocationMachineMasters ADD ClientId int NULL;
IF COL_LENGTH('dbo.UserAccessRights', 'ClientId') IS NULL ALTER TABLE dbo.UserAccessRights ADD ClientId int NULL;
IF COL_LENGTH('dbo.UserAccessRightMappings', 'ClientId') IS NULL ALTER TABLE dbo.UserAccessRightMappings ADD ClientId int NULL;
IF COL_LENGTH('dbo.FormResponses', 'ClientId') IS NULL ALTER TABLE dbo.FormResponses ADD ClientId int NULL;
IF COL_LENGTH('dbo.UserActivityLogs', 'ClientId') IS NULL ALTER TABLE dbo.UserActivityLogs ADD ClientId int NULL;
IF COL_LENGTH('dbo.UserAccessRightMappingLogs', 'ClientId') IS NULL ALTER TABLE dbo.UserAccessRightMappingLogs ADD ClientId int NULL;
IF OBJECT_ID('dbo.AuditLogs', 'U') IS NOT NULL AND COL_LENGTH('dbo.AuditLogs', 'ClientId') IS NULL ALTER TABLE dbo.AuditLogs ADD ClientId int NULL;
IF OBJECT_ID('dbo.ReportMessages', 'U') IS NOT NULL AND COL_LENGTH('dbo.ReportMessages', 'ClientId') IS NULL ALTER TABLE dbo.ReportMessages ADD ClientId int NULL;
IF OBJECT_ID('dbo.UserNotifications', 'U') IS NOT NULL AND COL_LENGTH('dbo.UserNotifications', 'ClientId') IS NULL ALTER TABLE dbo.UserNotifications ADD ClientId int NULL;

EXEC sys.sp_executesql N'UPDATE dbo.Users SET ClientId = @ClientId WHERE ClientId IS NULL;', N'@ClientId int', @DefaultClientId;
EXEC sys.sp_executesql N'UPDATE dbo.DropdownMasters SET ClientId = @ClientId WHERE ClientId IS NULL;', N'@ClientId int', @DefaultClientId;
EXEC sys.sp_executesql N'UPDATE dbo.LocationMachineMasters SET ClientId = @ClientId WHERE ClientId IS NULL;', N'@ClientId int', @DefaultClientId;
EXEC sys.sp_executesql N'UPDATE dbo.UserAccessRights SET ClientId = @ClientId WHERE ClientId IS NULL;', N'@ClientId int', @DefaultClientId;
EXEC sys.sp_executesql N'UPDATE m SET ClientId = u.ClientId FROM dbo.UserAccessRightMappings m INNER JOIN dbo.Users u ON u.UserId = m.UserId WHERE m.ClientId IS NULL;';
EXEC sys.sp_executesql N'UPDATE f SET ClientId = COALESCE(u.ClientId, @ClientId) FROM dbo.FormResponses f LEFT JOIN dbo.Users u ON u.UserId = f.ReportedBy WHERE f.ClientId IS NULL;', N'@ClientId int', @DefaultClientId;
EXEC sys.sp_executesql N'UPDATE l SET ClientId = COALESCE(u.ClientId, @ClientId) FROM dbo.UserActivityLogs l LEFT JOIN dbo.Users u ON u.UserId = l.UserId WHERE l.ClientId IS NULL;', N'@ClientId int', @DefaultClientId;
EXEC sys.sp_executesql N'UPDATE l SET ClientId = COALESCE(u.ClientId, @ClientId) FROM dbo.UserAccessRightMappingLogs l LEFT JOIN dbo.Users u ON u.UserId = l.UserId WHERE l.ClientId IS NULL;', N'@ClientId int', @DefaultClientId;
IF OBJECT_ID('dbo.AuditLogs', 'U') IS NOT NULL EXEC('UPDATE l SET ClientId = COALESCE(u.ClientId, (SELECT TOP (1) Id FROM dbo.ConfigureClients ORDER BY Id)) FROM dbo.AuditLogs l LEFT JOIN dbo.Users u ON u.UserId = l.ActorUserId WHERE l.ClientId IS NULL');
IF OBJECT_ID('dbo.ReportMessages', 'U') IS NOT NULL EXEC('UPDATE m SET ClientId = COALESCE(u.ClientId, (SELECT TOP (1) Id FROM dbo.ConfigureClients ORDER BY Id)) FROM dbo.ReportMessages m LEFT JOIN dbo.Users u ON u.UserId = m.SenderUserId WHERE m.ClientId IS NULL');
IF OBJECT_ID('dbo.UserNotifications', 'U') IS NOT NULL EXEC('UPDATE n SET ClientId = COALESCE(u.ClientId, (SELECT TOP (1) Id FROM dbo.ConfigureClients ORDER BY Id)) FROM dbo.UserNotifications n LEFT JOIN dbo.Users u ON u.UserId = n.UserId WHERE n.ClientId IS NULL');

EXEC('ALTER TABLE dbo.Users ALTER COLUMN ClientId int NOT NULL');
EXEC('ALTER TABLE dbo.DropdownMasters ALTER COLUMN ClientId int NOT NULL');
EXEC('ALTER TABLE dbo.LocationMachineMasters ALTER COLUMN ClientId int NOT NULL');
EXEC('ALTER TABLE dbo.UserAccessRights ALTER COLUMN ClientId int NOT NULL');
EXEC('ALTER TABLE dbo.UserAccessRightMappings ALTER COLUMN ClientId int NOT NULL');
EXEC('ALTER TABLE dbo.FormResponses ALTER COLUMN ClientId int NOT NULL');
EXEC('ALTER TABLE dbo.UserActivityLogs ALTER COLUMN ClientId int NOT NULL');
EXEC('ALTER TABLE dbo.UserAccessRightMappingLogs ALTER COLUMN ClientId int NOT NULL');
IF OBJECT_ID('dbo.AuditLogs', 'U') IS NOT NULL EXEC('ALTER TABLE dbo.AuditLogs ALTER COLUMN ClientId int NOT NULL');
IF OBJECT_ID('dbo.ReportMessages', 'U') IS NOT NULL EXEC('ALTER TABLE dbo.ReportMessages ALTER COLUMN ClientId int NOT NULL');
IF OBJECT_ID('dbo.UserNotifications', 'U') IS NOT NULL EXEC('ALTER TABLE dbo.UserNotifications ALTER COLUMN ClientId int NOT NULL');

-- Access-right names may repeat across clients, but not within one client.
DECLARE @oldAccessRightUniqueConstraint sysname;
SELECT TOP (1) @oldAccessRightUniqueConstraint = kc.name
FROM sys.key_constraints kc
JOIN sys.index_columns ic ON ic.object_id = kc.parent_object_id AND ic.index_id = kc.unique_index_id
JOIN sys.columns c ON c.object_id = ic.object_id AND c.column_id = ic.column_id
WHERE kc.parent_object_id = OBJECT_ID('dbo.UserAccessRights')
  AND kc.type = 'UQ' AND c.name = 'Name';
IF @oldAccessRightUniqueConstraint IS NOT NULL
BEGIN
    DECLARE @dropSql nvarchar(max) = N'ALTER TABLE dbo.UserAccessRights DROP CONSTRAINT ' + QUOTENAME(@oldAccessRightUniqueConstraint) + N';';
    EXEC sys.sp_executesql @dropSql;
END;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID('dbo.UserAccessRights') AND name = 'IX_UserAccessRights_ClientId_Name')
    EXEC('CREATE UNIQUE INDEX IX_UserAccessRights_ClientId_Name ON dbo.UserAccessRights(ClientId, Name)');

DECLARE @tables table (TableName sysname);
INSERT @tables VALUES ('Users'),('DropdownMasters'),('LocationMachineMasters'),('UserAccessRights'),('UserAccessRightMappings'),('FormResponses'),('UserActivityLogs'),('UserAccessRightMappingLogs'),('AuditLogs'),('ReportMessages'),('UserNotifications');
DECLARE @table sysname, @sql nvarchar(max);
DECLARE tenant_tables CURSOR LOCAL FAST_FORWARD FOR
    SELECT TableName FROM @tables WHERE OBJECT_ID('dbo.' + TableName, 'U') IS NOT NULL;
OPEN tenant_tables;
FETCH NEXT FROM tenant_tables INTO @table;
WHILE @@FETCH_STATUS = 0
BEGIN
    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_' + @table + '_ConfigureClients_ClientId')
    BEGIN
        SET @sql = N'ALTER TABLE dbo.' + QUOTENAME(@table) + N' WITH CHECK ADD CONSTRAINT ' +
            QUOTENAME('FK_' + @table + '_ConfigureClients_ClientId') +
            N' FOREIGN KEY (ClientId) REFERENCES dbo.ConfigureClients(Id);';
        EXEC sys.sp_executesql @sql;
    END;
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID('dbo.' + @table) AND name = 'IX_' + @table + '_ClientId')
    BEGIN
        SET @sql = N'CREATE INDEX ' + QUOTENAME('IX_' + @table + '_ClientId') + N' ON dbo.' + QUOTENAME(@table) + N'(ClientId);';
        EXEC sys.sp_executesql @sql;
    END;
    FETCH NEXT FROM tenant_tables INTO @table;
END;
CLOSE tenant_tables;
DEALLOCATE tenant_tables;

COMMIT TRANSACTION;
GO

-- Trigger-created rows must inherit the same tenant as their source user.
CREATE OR ALTER TRIGGER dbo.TR_Users_UserActivityLog
ON dbo.Users
AFTER UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    INSERT dbo.UserActivityLogs
        (ClientId, UserId, ActivityType, OldUsername, NewUsername, OldEmail, NewEmail,
         OldUserType, NewUserType, OldIsActive, NewIsActive, ActivityBy, HostName, ApplicationName)
    SELECT d.ClientId, d.UserId,
           CASE WHEN d.IsActive = 1 AND i.IsActive = 0 THEN 'Deleted' ELSE 'Updated' END,
           d.Username, i.Username, d.Email, i.Email, d.UserType, i.UserType,
           d.IsActive, i.IsActive, SUSER_SNAME(), HOST_NAME(), APP_NAME()
    FROM deleted d INNER JOIN inserted i ON i.UserId = d.UserId;

    INSERT dbo.UserActivityLogs
        (ClientId, UserId, ActivityType, OldUsername, OldEmail, OldUserType,
         OldIsActive, ActivityBy, HostName, ApplicationName)
    SELECT d.ClientId, d.UserId, 'Deleted', d.Username, d.Email, d.UserType,
           d.IsActive, SUSER_SNAME(), HOST_NAME(), APP_NAME()
    FROM deleted d LEFT JOIN inserted i ON i.UserId = d.UserId
    WHERE i.UserId IS NULL;
END;
GO

CREATE OR ALTER TRIGGER dbo.TR_UserAccessRightMappings_ActivityLog
ON dbo.UserAccessRightMappings
AFTER INSERT, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    INSERT dbo.UserAccessRightMappingLogs
        (ClientId, UserId, UserAccessRightId, AccessRightName, ActivityType, ActivityBy, HostName, ApplicationName)
    SELECT i.ClientId, i.UserId, i.UserAccessRightId, ar.Name, 'Assigned', SUSER_SNAME(), HOST_NAME(), APP_NAME()
    FROM inserted i INNER JOIN dbo.UserAccessRights ar ON ar.Id = i.UserAccessRightId;

    INSERT dbo.UserAccessRightMappingLogs
        (ClientId, UserId, UserAccessRightId, AccessRightName, ActivityType, ActivityBy, HostName, ApplicationName)
    SELECT d.ClientId, d.UserId, d.UserAccessRightId, ar.Name, 'Removed', SUSER_SNAME(), HOST_NAME(), APP_NAME()
    FROM deleted d INNER JOIN dbo.UserAccessRights ar ON ar.Id = d.UserAccessRightId;
END;
GO
