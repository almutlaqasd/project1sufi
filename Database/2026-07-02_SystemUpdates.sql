/* System updates: roles, audit, messaging, and notifications - 2026-07-02 */
SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF OBJECT_ID('dbo.AuditLogs', 'U') IS NULL
BEGIN
 CREATE TABLE dbo.AuditLogs(
  Id BIGINT IDENTITY(1,1) PRIMARY KEY, ActorUserId INT NULL, ActorName VARCHAR(50) NOT NULL,
  Action VARCHAR(20) NOT NULL, EntityType VARCHAR(50) NOT NULL, EntityId VARCHAR(50) NOT NULL,
  Changes NVARCHAR(MAX) NULL, CreatedOn DATETIME2 NOT NULL CONSTRAINT DF_AuditLogs_CreatedOn DEFAULT SYSDATETIME(),
  CONSTRAINT FK_AuditLogs_Users_ActorUserId FOREIGN KEY(ActorUserId) REFERENCES dbo.Users(UserId) ON DELETE SET NULL
 );
 CREATE INDEX IX_AuditLogs_CreatedOn ON dbo.AuditLogs(CreatedOn DESC);
END;

IF OBJECT_ID('dbo.ReportMessages', 'U') IS NULL
BEGIN
 CREATE TABLE dbo.ReportMessages(
  Id BIGINT IDENTITY(1,1) PRIMARY KEY, FormResponseId INT NULL, SenderUserId INT NOT NULL,
  RecipientUserId INT NOT NULL, Message NVARCHAR(2000) NOT NULL,
  CreatedOn DATETIME2 NOT NULL CONSTRAINT DF_ReportMessages_CreatedOn DEFAULT SYSDATETIME(), ReadOn DATETIME2 NULL,
  CONSTRAINT FK_ReportMessages_FormResponses_FormResponseId FOREIGN KEY(FormResponseId) REFERENCES dbo.FormResponses(Id) ON DELETE CASCADE,
  CONSTRAINT FK_ReportMessages_Users_SenderUserId FOREIGN KEY(SenderUserId) REFERENCES dbo.Users(UserId),
  CONSTRAINT FK_ReportMessages_Users_RecipientUserId FOREIGN KEY(RecipientUserId) REFERENCES dbo.Users(UserId)
 );
 CREATE INDEX IX_ReportMessages_Participants ON dbo.ReportMessages(RecipientUserId, SenderUserId, CreatedOn DESC);
END;

IF OBJECT_ID('dbo.UserNotifications', 'U') IS NULL
BEGIN
 CREATE TABLE dbo.UserNotifications(
  Id BIGINT IDENTITY(1,1) PRIMARY KEY, UserId INT NOT NULL, Title NVARCHAR(150) NOT NULL,
  Message NVARCHAR(500) NOT NULL, Url NVARCHAR(500) NULL,
  CreatedOn DATETIME2 NOT NULL CONSTRAINT DF_UserNotifications_CreatedOn DEFAULT SYSDATETIME(), ReadOn DATETIME2 NULL,
  CONSTRAINT FK_UserNotifications_Users_UserId FOREIGN KEY(UserId) REFERENCES dbo.Users(UserId) ON DELETE CASCADE
 );
 CREATE INDEX IX_UserNotifications_UserId_ReadOn ON dbo.UserNotifications(UserId, ReadOn, CreatedOn DESC);
END;

/* Bootstrap one Super Admin from the oldest active Admin. Review this account after deployment. */
IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE UserType = 'SuperAdmin')
BEGIN
 UPDATE dbo.Users SET UserType = 'SuperAdmin'
 WHERE UserId = (SELECT TOP (1) UserId FROM dbo.Users WHERE UserType = 'Admin' AND IsActive = 1 ORDER BY UserId);
END;

COMMIT TRANSACTION;
