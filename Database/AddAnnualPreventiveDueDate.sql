USE HospitalDB;
GO

IF COL_LENGTH('LocationMachineMasters', 'AnnualPreventiveDueDate') IS NULL
    ALTER TABLE LocationMachineMasters ADD AnnualPreventiveDueDate DATE NULL;
GO

IF COL_LENGTH('FormResponses', 'AnnualPreventiveDueDate') IS NULL
    ALTER TABLE FormResponses ADD AnnualPreventiveDueDate DATE NULL;
GO
