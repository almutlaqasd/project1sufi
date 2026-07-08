USE HospitalDB;
GO

IF OBJECT_ID('LocationMachineMasters', 'U') IS NULL
BEGIN
    CREATE TABLE LocationMachineMasters
    (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        EchoMachine NVARCHAR(100) NOT NULL,
        ClinicalEngNumber VARCHAR(50) NOT NULL,
        Location NVARCHAR(100) NOT NULL,
        SerialNumber VARCHAR(100) NULL,
        PropertyTagNumber VARCHAR(100) NOT NULL,
        UserAccessRightId INT NULL,
        AnnualPreventiveDueDate DATE NULL,
        IsActive BIT NOT NULL DEFAULT 1
    );
END
GO

IF COL_LENGTH('LocationMachineMasters', 'AnnualPreventiveDueDate') IS NULL
    ALTER TABLE LocationMachineMasters ADD AnnualPreventiveDueDate DATE NULL;
GO

IF COL_LENGTH('LocationMachineMasters', 'UserAccessRightId') IS NULL
    ALTER TABLE LocationMachineMasters ADD UserAccessRightId INT NULL;
GO

IF OBJECT_ID('UserAccessRights', 'U') IS NOT NULL
AND OBJECT_ID('FK_LocationMachineMasters_UserAccessRights_UserAccessRightId', 'F') IS NULL
BEGIN
    ALTER TABLE LocationMachineMasters
    ADD CONSTRAINT FK_LocationMachineMasters_UserAccessRights_UserAccessRightId
    FOREIGN KEY (UserAccessRightId) REFERENCES UserAccessRights(Id) ON DELETE SET NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM LocationMachineMasters WHERE EchoMachine = 'GE Vivid E95' AND ClinicalEngNumber = '117412')
    INSERT INTO LocationMachineMasters (EchoMachine, ClinicalEngNumber, Location, SerialNumber, PropertyTagNumber, AnnualPreventiveDueDate)
    VALUES ('GE Vivid E95', '117412', 'Room 40', 'AU02574', 'M0066066', NULL);

IF NOT EXISTS (SELECT 1 FROM LocationMachineMasters WHERE EchoMachine = 'Philips Epiq 3' AND ClinicalEngNumber = '111763')
    INSERT INTO LocationMachineMasters (EchoMachine, ClinicalEngNumber, Location, SerialNumber, PropertyTagNumber, AnnualPreventiveDueDate)
    VALUES ('Philips Epiq 3', '111763', 'Room 40', 'US014B0467', 'M0045167', NULL);

IF NOT EXISTS (SELECT 1 FROM LocationMachineMasters WHERE EchoMachine = 'GE Vivid E95' AND ClinicalEngNumber = '117410')
    INSERT INTO LocationMachineMasters (EchoMachine, ClinicalEngNumber, Location, SerialNumber, PropertyTagNumber, AnnualPreventiveDueDate)
    VALUES ('GE Vivid E95', '117410', 'Room 43', NULL, 'M0066064', NULL);

IF NOT EXISTS (SELECT 1 FROM LocationMachineMasters WHERE EchoMachine = 'GE Vivid S70 - 17' AND ClinicalEngNumber = '19621')
    INSERT INTO LocationMachineMasters (EchoMachine, ClinicalEngNumber, Location, SerialNumber, PropertyTagNumber, AnnualPreventiveDueDate)
    VALUES ('GE Vivid S70 - 17', '19621', 'Room 40 (PO)', NULL, 'M0091628', NULL);

IF NOT EXISTS (SELECT 1 FROM LocationMachineMasters WHERE EchoMachine = 'GE Vivid S70- 24' AND ClinicalEngNumber = '19641')
    INSERT INTO LocationMachineMasters (EchoMachine, ClinicalEngNumber, Location, SerialNumber, PropertyTagNumber, AnnualPreventiveDueDate)
    VALUES ('GE Vivid S70- 24', '19641', 'Room 41', '120046S70N', 'M0091831', NULL);
GO

IF COL_LENGTH('FormResponses', 'Location') IS NULL
    ALTER TABLE FormResponses ADD Location NVARCHAR(100) NULL;

IF COL_LENGTH('FormResponses', 'EchoMachine') IS NULL
    ALTER TABLE FormResponses ADD EchoMachine NVARCHAR(100) NULL;

IF COL_LENGTH('FormResponses', 'ClinicalEngNumber') IS NULL
    ALTER TABLE FormResponses ADD ClinicalEngNumber VARCHAR(50) NULL;

IF COL_LENGTH('FormResponses', 'SerialNumber') IS NULL
    ALTER TABLE FormResponses ADD SerialNumber VARCHAR(100) NULL;

IF COL_LENGTH('FormResponses', 'PropertyTagNumber') IS NULL
    ALTER TABLE FormResponses ADD PropertyTagNumber VARCHAR(100) NULL;

IF COL_LENGTH('FormResponses', 'AnnualPreventiveDueDate') IS NULL
    ALTER TABLE FormResponses ADD AnnualPreventiveDueDate DATE NULL;
GO
