USE HospitalDB;
GO

IF COL_LENGTH('LocationMachineMasters', 'UserAccessRightId') IS NULL
BEGIN
    ALTER TABLE LocationMachineMasters ADD UserAccessRightId INT NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM UserAccessRights WHERE Id = 2)
BEGIN
    RAISERROR('UserAccessRights record with Id = 2 does not exist.', 16, 1);
    RETURN;
END
GO

IF NOT EXISTS (SELECT 1 FROM LocationMachineMasters WHERE ClinicalEngNumber = '117412' AND PropertyTagNumber = 'M0066066')
    INSERT INTO LocationMachineMasters (EchoMachine, ClinicalEngNumber, Location, SerialNumber, PropertyTagNumber, DateAcquired, Problems, DateReported, ActionTaken, FinalActionRecommendation, WeeklyFilterCleaningDone, AnnualDueDate, AnnualPreventiveDueDate, UserAccessRightId)
    VALUES ('GE Vivid E95', '117412', 'Room 40', 'AU02574', 'M0066066', '1-Mar-18', NULL, NULL, NULL, NULL, 'Yes', 'Apr-27', '2027-04-01', 2);

IF NOT EXISTS (SELECT 1 FROM LocationMachineMasters WHERE ClinicalEngNumber = '111763' AND PropertyTagNumber = 'M0045167')
    INSERT INTO LocationMachineMasters (EchoMachine, ClinicalEngNumber, Location, SerialNumber, PropertyTagNumber, DateAcquired, Problems, DateReported, ActionTaken, FinalActionRecommendation, WeeklyFilterCleaningDone, AnnualDueDate, AnnualPreventiveDueDate, UserAccessRightId)
    VALUES ('Philips Epiq 3', '111763', 'Room 40', 'US014B0467', 'M0045167', 'Dec-14', NULL, NULL, NULL, NULL, 'Yes', 'Apr-27', '2027-04-01', 2);

IF NOT EXISTS (SELECT 1 FROM LocationMachineMasters WHERE ClinicalEngNumber = '117410' AND PropertyTagNumber = 'M0066064')
    INSERT INTO LocationMachineMasters (EchoMachine, ClinicalEngNumber, Location, SerialNumber, PropertyTagNumber, DateAcquired, Problems, DateReported, ActionTaken, FinalActionRecommendation, WeeklyFilterCleaningDone, AnnualDueDate, AnnualPreventiveDueDate, UserAccessRightId)
    VALUES ('GE Vivid E95', '117410', 'Room 43', NULL, 'M0066064', '1-Mar-18', NULL, NULL, NULL, NULL, 'Yes', 'Sep-26', '2026-09-01', 2);

IF NOT EXISTS (SELECT 1 FROM LocationMachineMasters WHERE ClinicalEngNumber = '19621' AND PropertyTagNumber = 'M0091628')
    INSERT INTO LocationMachineMasters (EchoMachine, ClinicalEngNumber, Location, SerialNumber, PropertyTagNumber, DateAcquired, Problems, DateReported, ActionTaken, FinalActionRecommendation, WeeklyFilterCleaningDone, AnnualDueDate, AnnualPreventiveDueDate, UserAccessRightId)
    VALUES ('GE Vivid S70 - 17', '19621', 'Room 40 (PO)', NULL, 'M0091628', 'Jun-23', NULL, NULL, NULL, NULL, 'NA', 'Jul-26', '2026-07-01', 2);

IF NOT EXISTS (SELECT 1 FROM LocationMachineMasters WHERE ClinicalEngNumber = '19641' AND PropertyTagNumber = 'M0091831')
    INSERT INTO LocationMachineMasters (EchoMachine, ClinicalEngNumber, Location, SerialNumber, PropertyTagNumber, DateAcquired, Problems, DateReported, ActionTaken, FinalActionRecommendation, WeeklyFilterCleaningDone, AnnualDueDate, AnnualPreventiveDueDate, UserAccessRightId)
    VALUES ('GE Vivid S70- 24', '19641', 'Room 41', '120046S70N', 'M0091831', 'Jun-23', NULL, NULL, NULL, NULL, 'NA', 'Jul-26', '2026-07-01', 2);

IF NOT EXISTS (SELECT 1 FROM LocationMachineMasters WHERE ClinicalEngNumber = '19646' AND PropertyTagNumber = 'M0091833')
    INSERT INTO LocationMachineMasters (EchoMachine, ClinicalEngNumber, Location, SerialNumber, PropertyTagNumber, DateAcquired, Problems, DateReported, ActionTaken, FinalActionRecommendation, WeeklyFilterCleaningDone, AnnualDueDate, AnnualPreventiveDueDate, UserAccessRightId)
    VALUES ('GE Vivid S70 (From Peds )', '19646', 'Room 42', NULL, 'M0091833', '1-Jun-23', NULL, NULL, NULL, NULL, 'NA', 'Jul-26', '2026-07-01', 2);

IF NOT EXISTS (SELECT 1 FROM LocationMachineMasters WHERE ClinicalEngNumber = '10792' AND PropertyTagNumber = 'M0083539')
    INSERT INTO LocationMachineMasters (EchoMachine, ClinicalEngNumber, Location, SerialNumber, PropertyTagNumber, DateAcquired, Problems, DateReported, ActionTaken, FinalActionRecommendation, WeeklyFilterCleaningDone, AnnualDueDate, AnnualPreventiveDueDate, UserAccessRightId)
    VALUES ('UV Antigermix Disinfector', '10792', 'Disinfection Room', NULL, 'M0083539', '1-Mar-22', 'PM Due Date', '18-May-26', NULL, 'PM was updated to March, 2027', NULL, 'May-26', '2026-05-01', 2);

IF NOT EXISTS (SELECT 1 FROM LocationMachineMasters WHERE ClinicalEngNumber = '141' AND PropertyTagNumber = 'M0084246')
    INSERT INTO LocationMachineMasters (EchoMachine, ClinicalEngNumber, Location, SerialNumber, PropertyTagNumber, DateAcquired, Problems, DateReported, ActionTaken, FinalActionRecommendation, WeeklyFilterCleaningDone, AnnualDueDate, AnnualPreventiveDueDate, UserAccessRightId)
    VALUES ('Welch Allyn Vital Signs Monitor', '141', 'Room 40', NULL, 'M0084246', NULL, NULL, NULL, NULL, NULL, NULL, 'Mar-27', '2027-03-01', 2);

IF NOT EXISTS (SELECT 1 FROM LocationMachineMasters WHERE ClinicalEngNumber = '143' AND PropertyTagNumber = 'M0084245')
    INSERT INTO LocationMachineMasters (EchoMachine, ClinicalEngNumber, Location, SerialNumber, PropertyTagNumber, DateAcquired, Problems, DateReported, ActionTaken, FinalActionRecommendation, WeeklyFilterCleaningDone, AnnualDueDate, AnnualPreventiveDueDate, UserAccessRightId)
    VALUES ('Welch Allyn Vital Signs Monitor', '143', 'Room 41', NULL, 'M0084245', NULL, NULL, NULL, NULL, NULL, NULL, 'Mar-27', '2027-03-01', 2);

IF NOT EXISTS (SELECT 1 FROM LocationMachineMasters WHERE ClinicalEngNumber = '140' AND PropertyTagNumber = 'M0084248')
    INSERT INTO LocationMachineMasters (EchoMachine, ClinicalEngNumber, Location, SerialNumber, PropertyTagNumber, DateAcquired, Problems, DateReported, ActionTaken, FinalActionRecommendation, WeeklyFilterCleaningDone, AnnualDueDate, AnnualPreventiveDueDate, UserAccessRightId)
    VALUES ('Welch Allyn Vital Signs Monitor', '140', 'Room 42', NULL, 'M0084248', NULL, NULL, NULL, NULL, NULL, NULL, 'Mar-27', '2027-03-01', 2);

IF NOT EXISTS (SELECT 1 FROM LocationMachineMasters WHERE ClinicalEngNumber = '142' AND PropertyTagNumber = 'M0084247')
    INSERT INTO LocationMachineMasters (EchoMachine, ClinicalEngNumber, Location, SerialNumber, PropertyTagNumber, DateAcquired, Problems, DateReported, ActionTaken, FinalActionRecommendation, WeeklyFilterCleaningDone, AnnualDueDate, AnnualPreventiveDueDate, UserAccessRightId)
    VALUES ('Welch Allyn Vital Signs Monitor', '142', 'Room 43', NULL, 'M0084247', NULL, NULL, NULL, NULL, NULL, NULL, 'Mar-27', '2027-03-01', 2);

IF NOT EXISTS (SELECT 1 FROM LocationMachineMasters WHERE ClinicalEngNumber = '60242' AND PropertyTagNumber = 'M0012824')
    INSERT INTO LocationMachineMasters (EchoMachine, ClinicalEngNumber, Location, SerialNumber, PropertyTagNumber, DateAcquired, Problems, DateReported, ActionTaken, FinalActionRecommendation, WeeklyFilterCleaningDone, AnnualDueDate, AnnualPreventiveDueDate, UserAccessRightId)
    VALUES ('Weighing Scale', '60242', 'Corridor', '5002-6903', 'M0012824', NULL, NULL, NULL, NULL, NULL, NULL, 'Jun-27', '2027-06-01', 2);
GO
