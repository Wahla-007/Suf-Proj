# Database Schema Documentation

## Overview
The Mess Management System uses SQL Server with the following core tables:

---

## Tables

### 1. AspNetUsers
**Purpose**: Stores teacher and admin user information

| Column | Type | Description |
|--------|------|-------------|
| Id | NVARCHAR(450) | Primary Key - Unique user identifier |
| FullName | NVARCHAR(MAX) | Teacher's full name |
| JoinedDate | DATETIME2 | Date when teacher joined |
| IsPasswordChanged | BIT | Flag for first-time password change |

**Relationships**:
- One-to-Many with TeacherAttendance (TeacherId)
- One-to-Many with MonthlyBill (TeacherId)
- One-to-Many with WeeklyMenu (CreatedById)

**Sample Data**:
```sql
INSERT INTO AspNetUsers VALUES 
('TEACHER001', 'Rajesh Kumar', '2025-01-15', 0),
('TEACHER002', 'Priya Singh', '2025-02-01', 1),
('ADMIN001', 'Admin User', '2024-12-01', 1);
```

---

### 2. TeacherAttendance
**Purpose**: Tracks daily attendance and meal consumption

| Column | Type | Description |
|--------|------|-------------|
| Id | INT | Primary Key (Auto-increment) |
| TeacherId | NVARCHAR(450) | Foreign Key to AspNetUsers |
| Date | DATE | Attendance date |
| Breakfast | BIT | 1=Yes, 0=No, NULL=Not marked |
| Lunch | BIT | 1=Yes, 0=No, NULL=Not marked |
| Dinner | BIT | 1=Yes, 0=No, NULL=Not marked |
| MarkedBy | NVARCHAR(MAX) | Who marked this attendance |
| IsVerified | BIT | 1=Verified, 0=Rejected, NULL=Pending |
| VerificationNote | NVARCHAR(MAX) | Admin's notes on verification |
| VerifiedAt | DATETIME2 | When verification happened |

**Constraints**:
- Foreign Key: TeacherId → AspNetUsers(Id)
- Unique: (TeacherId, Date)

**Sample Data**:
```sql
INSERT INTO TeacherAttendance VALUES 
(1, 'TEACHER001', '2025-12-09', 1, 1, 0, 'Admin', 1, 'Verified', '2025-12-09 14:30:00'),
(2, 'TEACHER002', '2025-12-09', 1, 1, 1, 'Admin', NULL, NULL, NULL);
```

---

### 3. WeeklyMenu
**Purpose**: Stores weekly meal plan and pricing

| Column | Type | Description |
|--------|------|-------------|
| Id | INT | Primary Key (Auto-increment) |
| WeekStartDate | DATE | First day of the week |
| BreakfastRate | DECIMAL(10,2) | Breakfast price per meal |
| LunchRate | DECIMAL(10,2) | Lunch price per meal |
| DinnerRate | DECIMAL(10,2) | Dinner price per meal |
| CreatedById | NVARCHAR(450) | Foreign Key to AspNetUsers (Admin) |
| CreatedAt | DATETIME2 | When menu was created |

**Constraints**:
- Foreign Key: CreatedById → AspNetUsers(Id)
- All rates must be >= 0

**Sample Data**:
```sql
INSERT INTO WeeklyMenu VALUES 
(1, '2025-12-08', 50.00, 100.00, 80.00, 'ADMIN001', '2025-12-08 09:00:00'),
(2, '2025-12-01', 50.00, 100.00, 80.00, 'ADMIN001', '2025-12-01 09:00:00');
```

---

### 4. MonthlyBill
**Purpose**: Monthly billing records for each teacher

| Column | Type | Description |
|--------|------|-------------|
| Id | INT | Primary Key (Auto-increment) |
| TeacherId | NVARCHAR(450) | Foreign Key to AspNetUsers |
| BillingDate | DATETIME2 | Date bill was generated |
| BreakfastCount | INT | Number of breakfasts consumed |
| LunchCount | INT | Number of lunches consumed |
| DinnerCount | INT | Number of dinners consumed |
| TotalAmount | DECIMAL(10,2) | Total bill amount |
| IsPaid | BIT | 1=Paid, 0=Pending |

**Constraints**:
- Foreign Key: TeacherId → AspNetUsers(Id)
- All counts must be >= 0
- TotalAmount must be >= 0

**Calculated Field**:
```
TotalAmount = (BreakfastCount × BreakfastRate) 
            + (LunchCount × LunchRate) 
            + (DinnerCount × DinnerRate)
```

**Sample Data**:
```sql
INSERT INTO MonthlyBill VALUES 
(1, 'TEACHER001', '2025-12-01', 20, 22, 18, 3740.00, 0),
(2, 'TEACHER002', '2025-12-01', 18, 20, 20, 3660.00, 0);
```

---

## Entity Relationships

```
AspNetUsers (1) ─────── (Many) TeacherAttendance
      ├─ Id ◄─────────── TeacherId
      
AspNetUsers (1) ─────── (Many) MonthlyBill
      ├─ Id ◄─────────── TeacherId
      
AspNetUsers (1) ─────── (Many) WeeklyMenu
      ├─ Id ◄─────────── CreatedById
```

---

## Data Flow

### Daily Workflow
```
1. Admin marks attendance for teacher
   → TeacherAttendance record created
   → MarkedBy = Admin name
   → IsVerified = NULL (pending)

2. Admin verifies attendance
   → IsVerified = 1 (true)
   → VerificationNote = optional notes
   → VerifiedAt = current timestamp

3. Teacher views in portal
   → Sees status (Verified/Pending/Rejected)
   → Can see verification notes
```

### Monthly Billing Workflow
```
1. End of month
   → Count all TeacherAttendance records for the month
   → Filter by IsVerified = 1 (only verified records)
   
2. Calculate totals
   → Sum BreakfastCount, LunchCount, DinnerCount per teacher
   → Get latest WeeklyMenu rates
   → Calculate TotalAmount
   
3. Create MonthlyBill
   → Insert new record with counts and total
   → IsPaid = 0 (initially pending)
   
4. Teacher pays
   → IsPaid = 1 (marked as paid)
   
5. Unpaid bills carry to next month
   → Display previous month's unpaid bills
   → Add to current month's calculation
```

---

## Queries

### Get all attendance for a teacher
```sql
SELECT * FROM TeacherAttendance 
WHERE TeacherId = 'TEACHER001' 
ORDER BY Date DESC;
```

### Get verified attendance only
```sql
SELECT * FROM TeacherAttendance 
WHERE TeacherId = 'TEACHER001' AND IsVerified = 1 
ORDER BY Date DESC;
```

### Get pending verification
```sql
SELECT * FROM TeacherAttendance 
WHERE IsVerified IS NULL 
ORDER BY Date DESC;
```

### Get current month bills
```sql
SELECT mb.*, u.FullName 
FROM MonthlyBill mb
JOIN AspNetUsers u ON mb.TeacherId = u.Id
WHERE MONTH(mb.BillingDate) = MONTH(GETDATE())
  AND YEAR(mb.BillingDate) = YEAR(GETDATE())
ORDER BY mb.BillingDate DESC;
```

### Calculate total unpaid
```sql
SELECT TeacherId, SUM(TotalAmount) as UnpaidAmount 
FROM MonthlyBill 
WHERE IsPaid = 0 
GROUP BY TeacherId;
```

### Get last week's menu
```sql
SELECT TOP 1 * FROM WeeklyMenu 
ORDER BY WeekStartDate DESC;
```

### Get meals consumed by teacher (monthly)
```sql
SELECT 
    TeacherId,
    MONTH(BillingDate) as Month,
    SUM(BreakfastCount) as TotalBreakfast,
    SUM(LunchCount) as TotalLunch,
    SUM(DinnerCount) as TotalDinner
FROM MonthlyBill
WHERE YEAR(BillingDate) = YEAR(GETDATE())
GROUP BY TeacherId, MONTH(BillingDate)
ORDER BY TeacherId, Month DESC;
```

---

## Indexes (Recommended)

```sql
-- For fast teacher lookups
CREATE INDEX idx_TeacherAttendance_TeacherId 
ON TeacherAttendance(TeacherId);

-- For date-based queries
CREATE INDEX idx_TeacherAttendance_Date 
ON TeacherAttendance(Date);

-- For billing lookups
CREATE INDEX idx_MonthlyBill_TeacherId 
ON MonthlyBill(TeacherId);

-- For bill date queries
CREATE INDEX idx_MonthlyBill_BillingDate 
ON MonthlyBill(BillingDate);
```

---

## Connection Details

**Server**: (localdb)\MSSQLLocalDB
**Database**: mess
**Authentication**: Windows Authentication
**Connection String**: 
```
Server=(localdb)\MSSQLLocalDB;Database=mess;Trusted_Connection=True;TrustServerCertificate=True;
```

---

## Notes

1. **Date Storage**: 
   - Attendance dates use DATE type (no time)
   - Billing/verification timestamps use DATETIME2

2. **Bit Fields**:
   - 1 = True/Yes
   - 0 = False/No
   - NULL = Not set/Pending

3. **Calculation**:
   - All amounts are in Rupees (₹)
   - Decimal(10,2) allows up to 99,999,999.99

4. **Constraints**:
   - Teachers cannot have duplicate attendance for same date
   - All foreign keys prevent orphaned records
   - Cascading deletes preserve data integrity

5. **Performance**:
   - Index on TeacherId for fast filtering
   - Index on dates for range queries
   - Denormalized TotalAmount for faster bill display

---

**Last Updated**: December 9, 2025
**Version**: 1.0
