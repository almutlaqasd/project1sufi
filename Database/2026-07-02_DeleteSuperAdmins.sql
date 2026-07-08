/* Remove the two temporary Super Admin accounts created on 2026-07-02. */
SET XACT_ABORT ON;
BEGIN TRANSACTION;

DECLARE @UsersToDelete TABLE (UserId INT PRIMARY KEY);

INSERT INTO @UsersToDelete (UserId)
SELECT UserId
FROM dbo.Users
WHERE Username IN ('superadmin', 'superadmin2')
  AND UserType = 'SuperAdmin';

/* These relationships use NO ACTION, so their dependent rows must be removed first. */
DELETE message
FROM dbo.ReportMessages AS message
WHERE message.SenderUserId IN (SELECT UserId FROM @UsersToDelete)
   OR message.RecipientUserId IN (SELECT UserId FROM @UsersToDelete);

/* Remaining user relationships are CASCADE or SET NULL. */
DELETE userAccount
FROM dbo.Users AS userAccount
WHERE userAccount.UserId IN (SELECT UserId FROM @UsersToDelete);

COMMIT TRANSACTION;
