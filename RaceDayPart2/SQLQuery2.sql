
UPDATE dbo.Users
SET Role = 'Organiser'
WHERE Email = 'MyOrganizerAccount@gmail.com';

-- Confirm the change
SELECT Id, Username, Email, Role
FROM dbo.Users
WHERE Email = 'MyOrganizerAccount@gmail.com';