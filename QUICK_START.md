# Quick Start Guide - Mess Management System

## 🚀 Get Started in 3 Steps

### Step 1: Start the Database
```powershell
sqllocaldb start MSSQLLocalDB
```

### Step 2: Run the Application
```bash
cd c:\Users\HP\source\repos\mess_management\mess_management
dotnet run
```

### Step 3: Open in Browser
Visit: `https://localhost:7xxx` (check terminal for exact port)

---

## 🎯 Main Portals

### Admin Portal
**Access**: Click "Go to Admin Dashboard" on home page

**Features**:
- 👥 **User Management**: Create/Edit/Delete teachers
- 📅 **Weekly Menu**: Set meal prices
- ✅ **Attendance**: Mark and verify daily attendance
- 💵 **Billing**: View and manage monthly bills

### Teacher Portal
**Access**: Click "Go to Teacher Dashboard" on home page

**Features**:
- 📊 **Dashboard**: Overview of stats
- 📅 **Attendance History**: View all records
- 💵 **My Bills**: Check charges
- 🍽️ **Weekly Menu**: See meal rates

---

## 📱 Navigation

### From Home Page
```
Home Page
├── Admin Panel (Left Card)
│   ├── Users
│   ├── Menus
│   ├── Attendance
│   └── Bills
└── Teacher Portal (Right Card)
    ├── Attendance
    ├── Bills
    └── Menu
```

### From Navigation Bar
```
Navbar
├── Admin Panel (Dropdown)
│   ├── Dashboard
│   ├── Users
│   ├── Weekly Menus
│   ├── Attendance
│   └── Bills
└── Teacher Portal
    └── Dashboard
```

---

## 💻 Common Tasks

### Add a New Teacher (Admin)
1. Go to Admin Panel → Users
2. Click "Create New"
3. Enter teacher details
4. Click "Create"

### Add Weekly Menu (Admin)
1. Go to Admin Panel → Weekly Menus
2. Click "Create New"
3. Set rates for Breakfast, Lunch, Dinner
4. Click "Create"

### Mark Attendance (Admin)
1. Go to Admin Panel → Attendance
2. Click "Create New"
3. Select teacher and date
4. Mark meal consumption (Breakfast/Lunch/Dinner)
5. Click "Create"

### Verify Attendance (Admin)
1. Go to Admin Panel → Attendance
2. Click "Edit" on pending record
3. Mark as verified
4. Add notes if needed
5. Click "Save"

### View Attendance (Teacher)
1. Go to Teacher Portal
2. Click "View Attendance"
3. Check status (Verified/Pending)

### Check Bills (Teacher)
1. Go to Teacher Portal
2. Click "View Bills"
3. See payment status
4. Check due amounts

---

## 🛠️ Troubleshooting

### Error: "Unable to resolve service for AppDbContext"
**Solution**: Restart the application
```bash
# Stop current run (Ctrl+C)
# Then run again
dotnet run
```

### Error: "Cannot connect to LocalDB"
**Solution**: Start LocalDB
```powershell
sqllocaldb start MSSQLLocalDB
# Check status
sqllocaldb info MSSQLLocalDB
```

### Port already in use
**Solution**: Use different port
```bash
dotnet run --urls "https://localhost:8080"
```

### Database not found
**Solution**: Check database exists
```powershell
sqlcmd -S "(localdb)\MSSQLLocalDB" -Q "SELECT name FROM sys.databases;"
```

---

## 📊 Data Fields

### Teacher Attendance
- **Date**: When attendance was marked
- **Breakfast/Lunch/Dinner**: Yes/No/Not marked
- **Marked By**: Who marked it
- **Status**: Verified/Pending/Rejected
- **Notes**: Verification comments

### Monthly Bill
- **Month**: Billing period
- **Meal Counts**: How many meals taken
- **Total Amount**: Calculated charges
- **Status**: Paid/Pending

### Weekly Menu
- **Week Starting**: First day of week
- **Rates**: Breakfast/Lunch/Dinner prices
- **Created By**: Admin who created it

---

## 💡 Tips

1. **Check Dashboard First**: Get overview before diving into details
2. **Verify Attendance**: Admin should verify records regularly
3. **Review Bills**: Check calculations before finalizing
4. **Track Payments**: Keep payment status updated
5. **Use Dates**: Filter by date range for reports

---

## 🔗 File Locations

- **Main Application**: `Program.cs`
- **Database Config**: `Models/AppDbContext.cs`
- **Admin Views**: `Views/Home/`, `Views/AspNetUser/`, etc.
- **Teacher Views**: `Views/Teacher/`
- **Styles**: `wwwroot/css/site.css`
- **Documentation**: `README.md`, `PROJECT_COMPLETION_REPORT.md`

---

## ✅ Checklist for Setup

- [ ] LocalDB is running
- [ ] Project builds without errors
- [ ] Application starts successfully
- [ ] Home page loads with both portals
- [ ] Can access Admin Panel
- [ ] Can access Teacher Portal
- [ ] Can navigate between pages

---

## 📞 Quick Help

**Can't see Admin features?**
- Verify you're logged in with admin account
- Check database has data

**Attendance not showing?**
- Ensure teacher is created first
- Check attendance is marked for that date

**Bills not calculating?**
- Verify menu rates are set
- Check attendance records exist

**Navigation menu not working?**
- Clear browser cache
- Refresh page
- Restart application

---

**Last Updated**: December 9, 2025
**Version**: 1.0 - Complete
