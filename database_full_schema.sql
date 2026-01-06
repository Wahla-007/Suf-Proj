-- Mess Management System - Full SQL Schema

-- 1. AspNetUsers (Core User Table)
CREATE TABLE [AspNetUsers] (
    [id] nvarchar(450) NOT NULL PRIMARY KEY,
    [Email] nvarchar(256) NULL,
    [PasswordHash] nvarchar(256) NULL,
    [FullName] nvarchar(max) NULL,
    [JoinedDate] datetime NULL,
    [IsAdmin] bit NOT NULL DEFAULT 0,
    [IsPasswordChanged] bit NOT NULL DEFAULT 0
);
GO

-- 2. WeeklyPlans (Manages weekly schedules)
CREATE TABLE [WeeklyPlans] (
    [Id] int IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [WeekStart] date NOT NULL,
    [CreatedAt] datetime NOT NULL DEFAULT GETDATE(),
    [CreatedById] nvarchar(450) NULL,
    CONSTRAINT [FK_WeeklyPlans_AspNetUsers] FOREIGN KEY ([CreatedById]) REFERENCES [AspNetUsers] ([id])
);
GO

-- 3. WeeklyPlanDays (Specific meals for each day)
CREATE TABLE [WeeklyPlanDays] (
    [Id] int IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [WeeklyPlanId] int NOT NULL,
    [DayOfWeek] int NOT NULL,
    [BreakfastName] nvarchar(50) DEFAULT 'Breakfast',
    [LunchName] nvarchar(50) DEFAULT 'Lunch',
    [DinnerName] nvarchar(50) DEFAULT 'Dinner',
    [BreakfastPrice] decimal(18,2) NOT NULL,
    [LunchPrice] decimal(18,2) NOT NULL,
    [DinnerPrice] decimal(18,2) NOT NULL,
    CONSTRAINT [FK_WeeklyPlanDays_WeeklyPlans] FOREIGN KEY ([WeeklyPlanId]) REFERENCES [WeeklyPlans] ([Id]) ON DELETE CASCADE
);
GO

-- 4. TeacherAttendance (Tracks daily meal logs)
CREATE TABLE [TeacherAttendance] (
    [id] int IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [TeacherId] nvarchar(450) NULL,
    [Date] date NULL,
    [Breakfast] bit NULL,
    [Lunch] bit NULL,
    [Dinner] bit NULL,
    [MarkedBy] nvarchar(256) DEFAULT 'System',
    [IsVerified] bit DEFAULT 0,
    [VerificationNote] nvarchar(1000) NULL,
    [VerifiedAt] datetime NULL,
    [DisputeStatus] nvarchar(50) DEFAULT 'None',
    [DisputeReason] nvarchar(1000) NULL,
    CONSTRAINT [FK_TeacherAttendance_AspNetUsers] FOREIGN KEY ([TeacherId]) REFERENCES [AspNetUsers] ([id])
);
GO

-- 5. Bills (Main billing table)
CREATE TABLE [Bills] (
    [Id] int IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [TeacherId] nvarchar(450) NULL,
    [Year] int NOT NULL,
    [Month] int NOT NULL,
    [TotalMealsAmount] decimal(18,2) NOT NULL,
    [WaterFee] decimal(18,2) NOT NULL,
    [PreviousDue] decimal(18,2) NOT NULL,
    [TotalDue] decimal(18,2) NOT NULL,
    [PaidAmount] decimal(18,2) NOT NULL,
    [Status] nvarchar(max) DEFAULT 'Pending',
    [GeneratedOn] datetime NOT NULL DEFAULT GETDATE(),
    [PaidOn] datetime NULL,
    CONSTRAINT [FK_Bills_AspNetUsers] FOREIGN KEY ([TeacherId]) REFERENCES [AspNetUsers] ([id])
);
GO

-- 6. BillLines (Itemized lines for each bill)
CREATE TABLE [BillLines] (
    [Id] int IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [BillId] int NOT NULL,
    [Date] date NOT NULL,
    [Description] nvarchar(max) NULL,
    [Price] decimal(18,2) NOT NULL,
    CONSTRAINT [FK_BillLines_Bills] FOREIGN KEY ([BillId]) REFERENCES [Bills] ([Id]) ON DELETE CASCADE
);
GO

-- 7. Payments (Transaction history)
CREATE TABLE [Payments] (
    [Id] int IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [BillId] int NOT NULL,
    [Amount] decimal(18,2) NOT NULL,
    [Method] nvarchar(max) NULL,
    [TransactionId] nvarchar(max) NULL,
    [PaidOn] datetime NOT NULL DEFAULT GETDATE(),
    CONSTRAINT [FK_Payments_Bills] FOREIGN KEY ([BillId]) REFERENCES [Bills] ([Id]) ON DELETE CASCADE
);
GO

-- 8. MonthlyBill (Legacy/Alternative billing summary)
CREATE TABLE [MonthlyBill] (
    [id] int IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [TeacherId] nvarchar(450) NULL,
    [Year] int NULL,
    [Month] int NULL,
    [TotalMeals] int NULL,
    [FoodAmount] decimal(18,0) NULL,
    [WaterShare] decimal(18,0) NULL,
    [PreviousDue] decimal(18,0) NULL,
    [TotalDue] decimal(18,0) NULL,
    [PaidAmount] decimal(18,0) NULL,
    [Status] nvarchar(50) DEFAULT 'Pending',
    [GeneratedOn] datetime DEFAULT GETDATE(),
    [PaidOn] datetime NULL,
    [PaymentToken] nvarchar(256) NULL,
    CONSTRAINT [FK_MonthlyBill_AspNetUsers] FOREIGN KEY ([TeacherId]) REFERENCES [AspNetUsers] ([id])
);
GO

-- 9. WeeklyMenu (Standard rates management)
CREATE TABLE [WeeklyMenu] (
    [id] int IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [WeekStartDate] date NULL,
    [BreakfastRate] decimal(18,2) NULL,
    [LunchRate] decimal(18,2) NULL,
    [DinnerRate] decimal(18,2) NULL,
    [CreatedById] nvarchar(450) NULL,
    [CreatedAt] datetime DEFAULT GETDATE(),
    CONSTRAINT [FK_WeeklyMenu_AspNetUsers] FOREIGN KEY ([CreatedById]) REFERENCES [AspNetUsers] ([id])
);
GO

-- 10. ReviewRequests (Dispute/Review management)
CREATE TABLE [ReviewRequests] (
    [Id] int IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [BillLineId] int NOT NULL,
    [UserId] nvarchar(max) NULL,
    [Reason] nvarchar(max) NULL,
    [Status] nvarchar(max) DEFAULT 'Open',
    [CreatedAt] datetime NOT NULL DEFAULT GETDATE(),
    CONSTRAINT [FK_ReviewRequests_BillLines] FOREIGN KEY ([BillLineId]) REFERENCES [BillLines] ([Id]) ON DELETE CASCADE
);
GO

-- 11. Settings (Application configuration)
CREATE TABLE [Settings] (
    [Key] nvarchar(450) NOT NULL PRIMARY KEY,
    [Value] nvarchar(max) NULL
);
GO

-- 12. RefreshTokens (JWT session management)
CREATE TABLE [RefreshTokens] (
    [Id] int IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [Token] nvarchar(256) NOT NULL,
    [UserId] nvarchar(450) NOT NULL,
    [CreatedAt] datetime NOT NULL,
    [ExpiresAt] datetime NOT NULL,
    [RevokedAt] datetime NULL,
    [IsRevoked] bit NOT NULL DEFAULT 0,
    CONSTRAINT [FK_RefreshTokens_AspNetUsers] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([id]) ON DELETE CASCADE
);
GO
CREATE UNIQUE INDEX [IX_RefreshTokens_Token] ON [RefreshTokens] ([Token]);
GO
