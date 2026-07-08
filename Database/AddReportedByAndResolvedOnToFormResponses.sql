USE HospitalDB;
GO

IF COL_LENGTH('FormResponses', 'ReportedBy') IS NULL
BEGIN
    ALTER TABLE FormResponses ADD ReportedBy INT NULL;
END
GO

IF COL_LENGTH('FormResponses', 'ResolvedOn') IS NULL
BEGIN
    ALTER TABLE FormResponses ADD ResolvedOn DATETIME NULL;
END
GO

IF OBJECT_ID('FK_FormResponses_Users_ReportedBy', 'F') IS NULL
BEGIN
    ALTER TABLE FormResponses
    ADD CONSTRAINT FK_FormResponses_Users_ReportedBy
    FOREIGN KEY (ReportedBy) REFERENCES Users(UserId) ON DELETE SET NULL;
END
GO

UPDATE FormResponses
SET ResolvedOn = CreatedOn
WHERE ResolvedStatus = 'Resolved' AND ResolvedOn IS NULL;
GO
