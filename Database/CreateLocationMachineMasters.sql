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
        DateAcquired NVARCHAR(50) NULL,
        Problems NVARCHAR(500) NULL,
        DateReported NVARCHAR(50) NULL,
        ActionTaken NVARCHAR(500) NULL,
        FinalActionRecommendation NVARCHAR(500) NULL,
        WeeklyFilterCleaningDone VARCHAR(20) NULL,
        AnnualDueDate NVARCHAR(50) NULL,
        UserAccessRightId INT NULL,
        AnnualPreventiveDueDate DATE NULL,
        IsActive BIT NOT NULL DEFAULT 1
    );
END
GO

IF COL_LENGTH('LocationMachineMasters', 'DateAcquired') IS NULL
    ALTER TABLE LocationMachineMasters ADD DateAcquired NVARCHAR(50) NULL;
IF COL_LENGTH('LocationMachineMasters', 'Problems') IS NULL
    ALTER TABLE LocationMachineMasters ADD Problems NVARCHAR(500) NULL;
IF COL_LENGTH('LocationMachineMasters', 'DateReported') IS NULL
    ALTER TABLE LocationMachineMasters ADD DateReported NVARCHAR(50) NULL;
IF COL_LENGTH('LocationMachineMasters', 'ActionTaken') IS NULL
    ALTER TABLE LocationMachineMasters ADD ActionTaken NVARCHAR(500) NULL;
IF COL_LENGTH('LocationMachineMasters', 'FinalActionRecommendation') IS NULL
    ALTER TABLE LocationMachineMasters ADD FinalActionRecommendation NVARCHAR(500) NULL;
IF COL_LENGTH('LocationMachineMasters', 'WeeklyFilterCleaningDone') IS NULL
    ALTER TABLE LocationMachineMasters ADD WeeklyFilterCleaningDone VARCHAR(20) NULL;
IF COL_LENGTH('LocationMachineMasters', 'AnnualDueDate') IS NULL
    ALTER TABLE LocationMachineMasters ADD AnnualDueDate NVARCHAR(50) NULL;
IF COL_LENGTH('LocationMachineMasters', 'UserAccessRightId') IS NULL
    ALTER TABLE LocationMachineMasters ADD UserAccessRightId INT NULL;
IF COL_LENGTH('LocationMachineMasters', 'AnnualPreventiveDueDate') IS NULL
    ALTER TABLE LocationMachineMasters ADD AnnualPreventiveDueDate DATE NULL;
IF COL_LENGTH('LocationMachineMasters', 'IsActive') IS NULL
    ALTER TABLE LocationMachineMasters ADD IsActive BIT NOT NULL CONSTRAINT DF_LocationMachineMasters_IsActive DEFAULT 1;
GO

IF OBJECT_ID('UserAccessRights', 'U') IS NOT NULL
AND OBJECT_ID('FK_LocationMachineMasters_UserAccessRights_UserAccessRightId', 'F') IS NULL
BEGIN
    ALTER TABLE LocationMachineMasters
    ADD CONSTRAINT FK_LocationMachineMasters_UserAccessRights_UserAccessRightId
    FOREIGN KEY (UserAccessRightId) REFERENCES UserAccessRights(Id) ON DELETE SET NULL;
END
GO

DELETE FROM LocationMachineMasters;
DBCC CHECKIDENT ('LocationMachineMasters', RESEED, 0);
GO

INSERT INTO LocationMachineMasters (EchoMachine, ClinicalEngNumber, Location, SerialNumber, PropertyTagNumber, DateAcquired, Problems, DateReported, ActionTaken, FinalActionRecommendation, WeeklyFilterCleaningDone, AnnualDueDate, AnnualPreventiveDueDate)
VALUES
('Vivid E95 cSound-14', '18016', 'Code 5', 'AU71711', 'M0081171', '2-Dec-20', NULL, NULL, NULL, NULL, 'yes', 'Feb-27', '2027-02-01'),
('VIVID S70 - 23', '19639', 'RM. 12', '123791570N', 'M0091824', 'Jun-23', NULL, NULL, NULL, NULL, 'NA', 'Jul-26', '2026-07-01'),
('Vivid E95 cSound-13', '18031', 'Rm. 13', 'AU71672', 'M0081163', '2-Dec-20', NULL, NULL, NULL, NULL, 'yes', 'Mar-27', '2027-03-01'),
('VIVID E95', '117411', 'ACHD', 'AU2586', 'M0066065', 'Mar-18', 'PM UPDATE REQUESTED', '18-May-26', NULL, 'PM UPDATED to May 2027', 'yes', 'May-26', '2026-05-01'),
('VIVID E95 - 11', '117479', 'Rm. 15', 'AU00992', 'M0056489', 'May-16', NULL, NULL, NULL, NULL, 'yes', 'Nov-26', '2026-11-01'),
('VIVID S70 - 25', '3601', 'Rm. 17', '125014570N', 'M0092110', '20-Sep-23', NULL, NULL, NULL, NULL, 'NA', 'Jul-26', '2026-07-01'),
('Epiq CVx 3D - 11', '13659', 'Rm. 19', 'USN 2281725', 'M0089432', '1-Dec-22', NULL, NULL, NULL, 'PM was updated to Jan, 2027', 'yes', 'Jan-26', '2026-01-01'),
('Vivid E95 cSound-15', '18026', 'Rm. 21', 'AU71675', 'M0081180', '2-Dec-20', 'the device is freezing', '18/05/2026', 'done', NULL, 'yes', 'Mar-27', '2027-03-01'),
('Vivid E95-10', '117477', 'Private Wing', 'AU1034', 'M0056487', 'May-16', NULL, NULL, NULL, NULL, 'yes', 'Dec-24', '2024-12-01'),
('Vivid E95 cSound-12', 'CE000909', 'Rm. 25', 'AU70330', 'M0076205', 'Sept. 2019', 'PM UPDATE REQUESTED', '18-May-26', 'Biomed has been called twice 1st on may18,2026 second on june2, 2026', 'PM UPDATED to June 2027', 'yes', 'May-26', '2026-05-01'),
('VIVID S70 - 20 (PO 1)', '19630', '4th Floor', '123835570N', 'M0091829', '14-Jun-23', NULL, NULL, NULL, NULL, 'NA', 'Jul-26', '2026-07-01'),
('VIVID S70 - 21 (PO 2)', '19633', '4th Floor', '123813570N', 'M0091825', '14-Jun-23', NULL, NULL, NULL, NULL, 'NA', 'Jul-26', '2026-07-01'),
('VIVID S70 - 22 (PO 3)', '19636', '4th Floor', '123821570N', 'M0091826', '14-Jun-23', NULL, NULL, NULL, NULL, 'NA', 'Jul-26', '2026-07-01'),
('Epiq CVx 3D - 05 ( PO 4 )', '9406', 'OR Level 4: Rm. 2', 'US720B1177', 'M0080040', '6-Oct-20', NULL, NULL, NULL, NULL, 'yes', 'Jul-26', '2026-07-01'),
('Epiq CVx 3D - 08 ( PO 5 )', '13546', 'Cath Lab Hybrid', 'USN 22B0857', 'M0084620', '7-Nov-22', NULL, NULL, NULL, NULL, 'yes', 'Oct-26', '2026-10-01'),
('Epiq CVx 3D - 09 ( PO 6 )', '13548', 'CSICU - Corridor', 'USN 22B0780', 'M0086939', 'Dec-22', NULL, NULL, NULL, NULL, 'yes', 'Oct-26', '2026-10-01'),
('VIVID S70 - 18 (PO 7)', '19624', 'EMS', '123785570N', 'M0091827', '14-Jun-23', NULL, NULL, NULL, NULL, 'NA', 'Jul-26', '2026-07-01'),
('VIVID S70 - 19 (PO 8)', '19627', 'East Wing', '123767570N', 'M0091830', '14-Jun-23', NULL, NULL, NULL, NULL, 'NA', 'Jul-26', '2026-07-01'),
('SIEMENS SC2000', '106724', '4th Floor (near stairway)', '401230', 'M0039465', 'Jan-13', NULL, NULL, NULL, NULL, 'yes', 'Aug-26', '2026-08-01'),
('Philips EPIQ -1', '111761', 'Adult NICL', 'US014B0469', 'M0045165', 'Dec. 2014', NULL, NULL, NULL, NULL, 'yes', 'Aug-26', '2026-08-01'),
('Philips EPIQ -2', '111762', 'Adult NICL', 'US014B0468', 'M0045166', 'Dec. 2014', NULL, NULL, NULL, NULL, 'yes', 'Aug-26', '2026-08-01'),
('EPIQ CVx 3D - 6', '9400', 'Adult NICL', 'US720VB1179', 'M0080041', '6-Oct-20', NULL, '12-Jun-25', 'WITH PEDS ECHO', NULL, 'yes', 'Jan-26', '2026-01-01'),
('EPIQ CVx 3D - 7', '9403', 'Adult NICL', 'US720B1180', 'M0080042', '6-Oct-20', NULL, NULL, NULL, NULL, 'yes', 'Jun-26', '2026-06-01'),
('EPIQ CVx 3D - 10', '13551', 'Adult NICL', 'USN 22B0779', 'M0086940', 'Dec-22', NULL, NULL, NULL, NULL, 'yes', 'Jan, 2027', '2027-01-01'),
('EPIQ CVx 3D - 11', '13659', 'Adult NICL', 'USN 22B1725', 'M0089432', 'Dec-22', NULL, NULL, NULL, NULL, 'yes', 'Jan-26', '2026-01-01'),
('Philips CX50', '9409', 'Adult NICL', 'SG77200130', 'M0080046', '6-Oct-20', NULL, NULL, NULL, NULL, 'NA', 'Mar-27', '2027-03-01'),
('VIVID IQ Premium R2 - 1', '26446', 'Adult NICL', '6107192WXO', 'M0091818', '14-Jun-23', NULL, NULL, NULL, NULL, 'NA', 'NO PM REQUIRED', NULL),
('VIVID IQ Premium R2 - 2', '26445', 'Adult NICL', '6107193WXO', 'M0091819', '14-Jun-23', NULL, NULL, NULL, NULL, 'NA', 'NO PM REQUIRED', NULL),
('ViVid E95 - 9', '117478', 'CIU', 'AU1027', 'M0056488', 'May-16', NULL, NULL, NULL, NULL, 'YES', 'Jul-26', '2026-07-01'),
('Vivid E95 cSound-16', '18021', 'Rm. 22', 'AU71676', 'M0081189', '2-Dec-20', NULL, NULL, NULL, NULL, 'YES', 'Mar-27', '2027-03-01'),
('Vivid E95 cSound', '1702', 'ACHD', '108173', 'M0076203', 'Sept. 2019', NULL, NULL, 'FROM PEDIATRIC ECHO', NULL, 'YES', 'Apr-27', '2027-04-01'),
('Epiq CVx - 12', '29269', 'Rm. 11', 'US725B0791', 'M0096359', '1-Sep-25', NULL, NULL, NULL, NULL, 'Yes', NULL, NULL),
('Vivid E95 cSound', '117476', '', 'AU01019', 'M0056486', '1-May-16', NULL, NULL, 'FROM PEDIATRIC ECHO', NULL, NULL, NULL, NULL),
('Welch Allyn Vital Signs Monitor', '1216', 'Rm. 11', NULL, 'M007649', NULL, NULL, NULL, NULL, NULL, NULL, 'Oct-27', '2027-10-01'),
('Welch Allyn Vital Signs Monitor', '1590', 'RM. 12', NULL, 'M0076483', NULL, NULL, NULL, NULL, NULL, NULL, 'Oct-27', '2027-10-01'),
('Welch Allyn Vital Signs Monitor', '1587', 'Rm. 13', NULL, 'M0076466', NULL, NULL, NULL, NULL, NULL, NULL, 'Oct-27', '2027-10-01'),
('Welch Allyn Vital Signs Monitor', '1588', 'Rm. 14', NULL, 'M0076467', NULL, NULL, NULL, NULL, NULL, NULL, 'Oct-27', '2027-10-01'),
('Welch Allyn Vital Signs Monitor', '1589', 'Rm. 15', NULL, 'M0076478', NULL, NULL, NULL, NULL, NULL, NULL, 'Oct-27', '2027-10-01'),
('Welch Allyn Vital Signs Monitor', '1586', 'Rm. 17', NULL, 'M0076472', NULL, NULL, NULL, NULL, NULL, NULL, 'Oct-27', '2027-10-01'),
('Welch Allyn Vital Signs Monitor', '1298', 'Rm. 21', NULL, 'M0076473', NULL, NULL, NULL, NULL, NULL, NULL, 'Dec-26', '2026-12-01'),
('Welch Allyn Vital Signs Monitor', '1584', 'Rm. 22', NULL, 'M0076475', NULL, NULL, NULL, NULL, NULL, NULL, 'Oct-27', '2027-10-01'),
('UV Antigermix Disinfector', '1709', 'Disinfection Room', NULL, 'M0076712', NULL, NULL, NULL, NULL, NULL, NULL, 'Aug-26', '2026-08-01'),
('Weighing Scale', '104511', 'Corridor', '5002-27342', 'M0037566', NULL, NULL, NULL, NULL, NULL, NULL, 'Mar-26', '2026-03-01');
GO
