USE HospitalDB;
GO

IF COL_LENGTH('Users', 'IsActive') IS NULL
BEGIN
    ALTER TABLE Users
    ADD IsActive BIT NOT NULL CONSTRAINT DF_Users_IsActive DEFAULT 1;
END
GO

IF OBJECT_ID('UserAccessRights', 'U') IS NULL
BEGIN
    CREATE TABLE UserAccessRights
    (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Name NVARCHAR(100) NOT NULL UNIQUE,
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedOn DATETIME NOT NULL DEFAULT GETDATE()
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM UserAccessRights WHERE Name = 'Adult')
    INSERT INTO UserAccessRights (Name) VALUES ('Adult');

IF NOT EXISTS (SELECT 1 FROM UserAccessRights WHERE Name = 'Pediatric')
    INSERT INTO UserAccessRights (Name) VALUES ('Pediatric');

IF NOT EXISTS (SELECT 1 FROM UserAccessRights WHERE Name = 'KACOLD')
    INSERT INTO UserAccessRights (Name) VALUES ('KACOLD');
GO

IF OBJECT_ID('UserAccessRightMappings', 'U') IS NULL
BEGIN
    CREATE TABLE UserAccessRightMappings
    (
        UserId INT NOT NULL,
        UserAccessRightId INT NOT NULL,
        CONSTRAINT PK_UserAccessRightMappings PRIMARY KEY (UserId, UserAccessRightId),
        CONSTRAINT FK_UserAccessRightMappings_Users_UserId FOREIGN KEY (UserId) REFERENCES Users(UserId) ON DELETE CASCADE,
        CONSTRAINT FK_UserAccessRightMappings_UserAccessRights_UserAccessRightId FOREIGN KEY (UserAccessRightId) REFERENCES UserAccessRights(Id) ON DELETE CASCADE
    );
END
GO

IF OBJECT_ID('UserActivityLogs', 'U') IS NULL
BEGIN
    CREATE TABLE UserActivityLogs
    (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        UserId INT NOT NULL,
        ActivityType VARCHAR(20) NOT NULL,
        OldUsername VARCHAR(50) NULL,
        NewUsername VARCHAR(50) NULL,
        OldEmail VARCHAR(100) NULL,
        NewEmail VARCHAR(100) NULL,
        OldUserType VARCHAR(20) NULL,
        NewUserType VARCHAR(20) NULL,
        OldIsActive BIT NULL,
        NewIsActive BIT NULL,
        ActivityBy SYSNAME NULL,
        HostName NVARCHAR(128) NULL,
        ApplicationName NVARCHAR(128) NULL,
        ActivityOn DATETIME NOT NULL DEFAULT GETDATE()
    );
END
GO

CREATE OR ALTER TRIGGER TR_Users_UserActivityLog
ON Users
AFTER UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO UserActivityLogs
    (
        UserId,
        ActivityType,
        OldUsername,
        NewUsername,
        OldEmail,
        NewEmail,
        OldUserType,
        NewUserType,
        OldIsActive,
        NewIsActive,
        ActivityBy,
        HostName,
        ApplicationName
    )
    SELECT
        d.UserId,
        CASE
            WHEN d.IsActive = 1 AND i.IsActive = 0 THEN 'Deleted'
            ELSE 'Updated'
        END,
        d.Username,
        i.Username,
        d.Email,
        i.Email,
        d.UserType,
        i.UserType,
        d.IsActive,
        i.IsActive,
        SUSER_SNAME(),
        HOST_NAME(),
        APP_NAME()
    FROM deleted d
    INNER JOIN inserted i ON i.UserId = d.UserId;

    INSERT INTO UserActivityLogs
    (
        UserId,
        ActivityType,
        OldUsername,
        OldEmail,
        OldUserType,
        OldIsActive,
        ActivityBy,
        HostName,
        ApplicationName
    )
    SELECT
        d.UserId,
        'Deleted',
        d.Username,
        d.Email,
        d.UserType,
        d.IsActive,
        SUSER_SNAME(),
        HOST_NAME(),
        APP_NAME()
    FROM deleted d
    LEFT JOIN inserted i ON i.UserId = d.UserId
    WHERE i.UserId IS NULL;
END
GO

IF OBJECT_ID('UserAccessRightMappingLogs', 'U') IS NULL
BEGIN
    CREATE TABLE UserAccessRightMappingLogs
    (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        UserId INT NOT NULL,
        UserAccessRightId INT NOT NULL,
        AccessRightName NVARCHAR(100) NULL,
        ActivityType VARCHAR(20) NOT NULL,
        ActivityBy SYSNAME NULL,
        HostName NVARCHAR(128) NULL,
        ApplicationName NVARCHAR(128) NULL,
        ActivityOn DATETIME NOT NULL DEFAULT GETDATE()
    );
END
GO

CREATE OR ALTER TRIGGER TR_UserAccessRightMappings_ActivityLog
ON UserAccessRightMappings
AFTER INSERT, DELETE
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO UserAccessRightMappingLogs
    (
        UserId,
        UserAccessRightId,
        AccessRightName,
        ActivityType,
        ActivityBy,
        HostName,
        ApplicationName
    )
    SELECT
        i.UserId,
        i.UserAccessRightId,
        ar.Name,
        'Assigned',
        SUSER_SNAME(),
        HOST_NAME(),
        APP_NAME()
    FROM inserted i
    LEFT JOIN UserAccessRights ar ON ar.Id = i.UserAccessRightId;

    INSERT INTO UserAccessRightMappingLogs
    (
        UserId,
        UserAccessRightId,
        AccessRightName,
        ActivityType,
        ActivityBy,
        HostName,
        ApplicationName
    )
    SELECT
        d.UserId,
        d.UserAccessRightId,
        ar.Name,
        'Removed',
        SUSER_SNAME(),
        HOST_NAME(),
        APP_NAME()
    FROM deleted d
    LEFT JOIN UserAccessRights ar ON ar.Id = d.UserAccessRightId;
END
GO
