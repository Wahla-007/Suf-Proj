-- Fix Login Credentials to Plain Text

-- Admin
UPDATE AspNetUsers 
SET PasswordHash = 'Admin@123', IsPasswordChanged = 1 
WHERE id = 'ADMIN';

-- Teachers
UPDATE AspNetUsers 
SET PasswordHash = 'Teacher@123', IsPasswordChanged = 1 
WHERE id LIKE 'TEACHER%';

-- Verify
SELECT id, Email, PasswordHash FROM AspNetUsers;
