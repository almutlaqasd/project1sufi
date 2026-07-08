USE HospitalDB;
GO

IF OBJECT_ID('ConfigureClients', 'U') IS NULL
BEGIN
    CREATE TABLE ConfigureClients
    (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        ProjectTitle NVARCHAR(150) NOT NULL,
        ClientName NVARCHAR(150) NULL,
        ContactNo VARCHAR(50) NULL,
        Email VARCHAR(150) NULL,
        Website VARCHAR(200) NULL,
        Address NVARCHAR(500) NULL,
        City NVARCHAR(100) NULL,
        Country NVARCHAR(100) NULL,
        SupportContact VARCHAR(50) NULL,
        SupportEmail VARCHAR(150) NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedOn DATETIME NOT NULL DEFAULT GETDATE(),
        UpdatedOn DATETIME NULL
    );
END
GO

IF COL_LENGTH('ConfigureClients', 'ClientName') IS NULL
    ALTER TABLE ConfigureClients ADD ClientName NVARCHAR(150) NULL;
IF COL_LENGTH('ConfigureClients', 'ContactNo') IS NULL
    ALTER TABLE ConfigureClients ADD ContactNo VARCHAR(50) NULL;
IF COL_LENGTH('ConfigureClients', 'Email') IS NULL
    ALTER TABLE ConfigureClients ADD Email VARCHAR(150) NULL;
IF COL_LENGTH('ConfigureClients', 'Website') IS NULL
    ALTER TABLE ConfigureClients ADD Website VARCHAR(200) NULL;
IF COL_LENGTH('ConfigureClients', 'Address') IS NULL
    ALTER TABLE ConfigureClients ADD Address NVARCHAR(500) NULL;
IF COL_LENGTH('ConfigureClients', 'City') IS NULL
    ALTER TABLE ConfigureClients ADD City NVARCHAR(100) NULL;
IF COL_LENGTH('ConfigureClients', 'Country') IS NULL
    ALTER TABLE ConfigureClients ADD Country NVARCHAR(100) NULL;
IF COL_LENGTH('ConfigureClients', 'SupportContact') IS NULL
    ALTER TABLE ConfigureClients ADD SupportContact VARCHAR(50) NULL;
IF COL_LENGTH('ConfigureClients', 'SupportEmail') IS NULL
    ALTER TABLE ConfigureClients ADD SupportEmail VARCHAR(150) NULL;
IF COL_LENGTH('ConfigureClients', 'IsActive') IS NULL
    ALTER TABLE ConfigureClients ADD IsActive BIT NOT NULL CONSTRAINT DF_ConfigureClients_IsActive DEFAULT 1;
IF COL_LENGTH('ConfigureClients', 'CreatedOn') IS NULL
    ALTER TABLE ConfigureClients ADD CreatedOn DATETIME NOT NULL CONSTRAINT DF_ConfigureClients_CreatedOn DEFAULT GETDATE();
IF COL_LENGTH('ConfigureClients', 'UpdatedOn') IS NULL
    ALTER TABLE ConfigureClients ADD UpdatedOn DATETIME NULL;
GO

IF NOT EXISTS (SELECT 1 FROM ConfigureClients)
BEGIN
    INSERT INTO ConfigureClients (ProjectTitle, ClientName, IsActive)
    VALUES ('Equipment Maintenance & Reporting System', 'Hospital CMMS', 1);
END
GO
