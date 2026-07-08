USE HospitalDB;
GO

IF OBJECT_ID('DropdownMasters', 'U') IS NULL
BEGIN
    CREATE TABLE DropdownMasters
    (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Category VARCHAR(50) NOT NULL,
        Name NVARCHAR(100) NOT NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        SortOrder INT NOT NULL DEFAULT 0
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM DropdownMasters WHERE Category = 'Type' AND Name = 'Equipment')
    INSERT INTO DropdownMasters (Category, Name, SortOrder) VALUES ('Type', 'Equipment', 1);

IF NOT EXISTS (SELECT 1 FROM DropdownMasters WHERE Category = 'Type' AND Name = 'Safety')
    INSERT INTO DropdownMasters (Category, Name, SortOrder) VALUES ('Type', 'Safety', 2);

IF NOT EXISTS (SELECT 1 FROM DropdownMasters WHERE Category = 'Device' AND Name = 'Echo')
    INSERT INTO DropdownMasters (Category, Name, SortOrder) VALUES ('Device', 'Echo', 1);

IF NOT EXISTS (SELECT 1 FROM DropdownMasters WHERE Category = 'Device' AND Name = 'TTE')
    INSERT INTO DropdownMasters (Category, Name, SortOrder) VALUES ('Device', 'TTE', 2);

IF NOT EXISTS (SELECT 1 FROM DropdownMasters WHERE Category = 'Device' AND Name = 'ECG')
    INSERT INTO DropdownMasters (Category, Name, SortOrder) VALUES ('Device', 'ECG', 3);

IF NOT EXISTS (SELECT 1 FROM DropdownMasters WHERE Category = 'Device' AND Name = 'Other')
    INSERT INTO DropdownMasters (Category, Name, SortOrder) VALUES ('Device', 'Other', 4);

IF NOT EXISTS (SELECT 1 FROM DropdownMasters WHERE Category = 'Priority' AND Name = 'Low')
    INSERT INTO DropdownMasters (Category, Name, SortOrder) VALUES ('Priority', 'Low', 1);

IF NOT EXISTS (SELECT 1 FROM DropdownMasters WHERE Category = 'Priority' AND Name = 'Medium')
    INSERT INTO DropdownMasters (Category, Name, SortOrder) VALUES ('Priority', 'Medium', 2);

IF NOT EXISTS (SELECT 1 FROM DropdownMasters WHERE Category = 'Priority' AND Name = 'High')
    INSERT INTO DropdownMasters (Category, Name, SortOrder) VALUES ('Priority', 'High', 3);
GO
