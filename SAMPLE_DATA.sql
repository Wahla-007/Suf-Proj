-- Sample Data for Mess Management System
-- Run this in SQL Server Management Studio against the 'mess' database

-- Clear existing data (if needed)
-- DELETE FROM MonthlyBill
-- DELETE FROM TeacherAttendance
-- DELETE FROM WeeklyMenu
-- DELETE FROM AspNetUsers

-- Insert Sample Teachers
INSERT INTO AspNetUsers (id, FullName, IsPasswordChanged, JoinedDate)
VALUES 
('TEACHER001', 'Dr. Rajesh Kumar', 1, '2024-01-15'),
('TEACHER002', 'Prof. Priya Singh', 1, '2024-02-20'),
('TEACHER003', 'Dr. Amit Patel', 1, '2024-03-10'),
('TEACHER004', 'Mrs. Neha Sharma', 1, '2024-04-05'),
('TEACHER005', 'Dr. Vikram Gupta', 1, '2024-05-12');

-- Insert Weekly Menu
INSERT INTO WeeklyMenu (WeekStartDate, BreakfastRate, LunchRate, DinnerRate, CreatedBy, CreatedAt)
VALUES 
('2024-12-02', 40.00, 80.00, 70.00, 'ADMIN', '2024-11-30 10:00:00'),
('2024-12-09', 45.00, 85.00, 75.00, 'ADMIN', '2024-12-02 10:00:00');

-- Insert Teacher Attendance
INSERT INTO TeacherAttendance (TeacherId, Date, Breakfast, Lunch, Dinner, MarkedBy, IsVerified, VerificationNote, VerifiedAt)
VALUES 
('TEACHER001', '2024-12-09', 1, 1, 1, 'ADMIN', 1, 'Verified', '2024-12-09 15:00:00'),
('TEACHER001', '2024-12-08', 1, 1, 0, 'ADMIN', 1, 'Verified', '2024-12-08 15:00:00'),
('TEACHER001', '2024-12-07', 1, 0, 1, 'ADMIN', NULL, NULL, NULL),
('TEACHER001', '2024-12-06', 1, 1, 1, 'ADMIN', 1, 'Verified', '2024-12-06 15:00:00'),

('TEACHER002', '2024-12-09', 1, 1, 1, 'ADMIN', 1, 'Verified', '2024-12-09 15:00:00'),
('TEACHER002', '2024-12-08', 1, 1, 1, 'ADMIN', 1, 'Verified', '2024-12-08 15:00:00'),
('TEACHER002', '2024-12-07', 0, 1, 1, 'ADMIN', 1, 'Verified', '2024-12-07 15:00:00'),

('TEACHER003', '2024-12-09', 1, 1, 0, 'ADMIN', NULL, NULL, NULL),
('TEACHER003', '2024-12-08', 1, 1, 1, 'ADMIN', 1, 'Verified', '2024-12-08 15:00:00'),
('TEACHER003', '2024-12-07', 1, 1, 1, 'ADMIN', 1, 'Verified', '2024-12-07 15:00:00'),

('TEACHER004', '2024-12-09', 1, 1, 1, 'ADMIN', 1, 'Verified', '2024-12-09 15:00:00'),
('TEACHER004', '2024-12-08', 1, 0, 1, 'ADMIN', NULL, NULL, NULL),
('TEACHER004', '2024-12-07', 1, 1, 1, 'ADMIN', 1, 'Verified', '2024-12-07 15:00:00'),

('TEACHER005', '2024-12-09', 1, 1, 1, 'ADMIN', 1, 'Verified', '2024-12-09 15:00:00'),
('TEACHER005', '2024-12-08', 1, 1, 1, 'ADMIN', 1, 'Verified', '2024-12-08 15:00:00'),
('TEACHER005', '2024-12-07', 0, 1, 1, 'ADMIN', 1, 'Verified', '2024-12-07 15:00:00');

-- Insert Monthly Bills
INSERT INTO MonthlyBill (TeacherId, Year, Month, TotalMeals, FoodAmount, WaterShare, PreviousDue, TotalDue, PaidAmount, Status, GeneratedOn, PaidOn, PaymentToken)
VALUES 
-- Teacher 001
('TEACHER001', 2024, 11, 85, 7650.00, 500.00, 0.00, 8150.00, 8150.00, 'Paid', '2024-11-30 09:00:00', '2024-12-05 14:30:00', 'PAY001'),
('TEACHER001', 2024, 12, 45, 4050.00, 500.00, 0.00, 4550.00, 0.00, 'Pending', '2024-12-09 09:00:00', NULL, NULL),

-- Teacher 002
('TEACHER002', 2024, 11, 88, 7920.00, 500.00, 0.00, 8420.00, 0.00, 'Pending', '2024-11-30 09:00:00', NULL, NULL),
('TEACHER002', 2024, 12, 47, 4230.00, 500.00, 8420.00, 13150.00, 5000.00, 'Pending', '2024-12-09 09:00:00', NULL, NULL),

-- Teacher 003
('TEACHER003', 2024, 11, 82, 7380.00, 500.00, 0.00, 7880.00, 7880.00, 'Paid', '2024-11-30 09:00:00', '2024-12-03 10:15:00', 'PAY002'),
('TEACHER003', 2024, 12, 43, 3870.00, 500.00, 0.00, 4370.00, 0.00, 'Pending', '2024-12-09 09:00:00', NULL, NULL),

-- Teacher 004
('TEACHER004', 2024, 11, 90, 8100.00, 500.00, 500.00, 9100.00, 9100.00, 'Paid', '2024-11-30 09:00:00', '2024-12-06 16:45:00', 'PAY003'),
('TEACHER004', 2024, 12, 46, 4140.00, 500.00, 0.00, 4640.00, 2000.00, 'Pending', '2024-12-09 09:00:00', NULL, NULL),

-- Teacher 005
('TEACHER005', 2024, 11, 86, 7740.00, 500.00, 0.00, 8240.00, 8240.00, 'Paid', '2024-11-30 09:00:00', '2024-12-04 11:20:00', 'PAY004'),
('TEACHER005', 2024, 12, 44, 3960.00, 500.00, 0.00, 4460.00, 0.00, 'Pending', '2024-12-09 09:00:00', NULL, NULL);

-- Verify inserted data
SELECT COUNT(*) as 'Total Teachers' FROM AspNetUsers;
SELECT COUNT(*) as 'Total Menu Records' FROM WeeklyMenu;
SELECT COUNT(*) as 'Total Attendance Records' FROM TeacherAttendance;
SELECT COUNT(*) as 'Total Bills' FROM MonthlyBill;
