-- Create Admin User if it doesn't exist
IF NOT EXISTS (SELECT 1 FROM AspNetUsers WHERE Email = 'admin@local')
BEGIN
    INSERT INTO AspNetUsers (id, Email, FullName, PasswordHash, IsAdmin, IsPasswordChanged, JoinedDate)
    VALUES ('ADMIN001', 'admin@local', 'Administrator', 'Admin@123', 1, 1, GETDATE())
    
    PRINT 'Admin user created successfully!'
    PRINT 'Email: admin@local'
    PRINT 'Password: Admin@123'
END
ELSE
BEGIN
    PRINT 'Admin user already exists. Updating password...'
    UPDATE AspNetUsers 
    SET PasswordHash = 'Admin@123', IsPasswordChanged = 1
    WHERE Email = 'admin@local'
END

-- Verify
SELECT id, Email, FullName, PasswordHash, IsAdmin FROM AspNetUsers WHERE Email = 'admin@local';
