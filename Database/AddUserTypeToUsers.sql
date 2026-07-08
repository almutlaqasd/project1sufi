USE HospitalDB;
GO

IF COL_LENGTH('Users', 'UserType') IS NULL
BEGIN
    ALTER TABLE Users
    ADD UserType VARCHAR(20) NOT NULL CONSTRAINT DF_Users_UserType DEFAULT 'User';
END
GO

UPDATE Users SET UserType = 'Admin' WHERE Username = 'admin';
UPDATE Users SET UserType = 'User' WHERE Username <> 'admin';
GO
