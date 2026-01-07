using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using mess_management.Models;

namespace mess_management.Controllers
{
    [Authorize]
    public class ReportsController : Controller
    {
        private readonly AppDbContext _context;

        public ReportsController(AppDbContext context)
        {
            _context = context;
        }

        #region Admin Reports

        // GET: Reports/AllBills - Download all bills as CSV (Admin only)
        public async Task<IActionResult> AllBills(int? year, int? month)
        {
            if (User.Claims.FirstOrDefault(c => c.Type == "isAdmin")?.Value != "true") return Forbid();

            var query = _context.Bills.Include(b => b.Teacher).Include(b => b.Lines).AsQueryable();
            
            if (year.HasValue) query = query.Where(b => b.Year == year.Value);
            if (month.HasValue) query = query.Where(b => b.Month == month.Value);

            var bills = await query.OrderByDescending(b => b.Year).ThenByDescending(b => b.Month).ToListAsync();

            var csv = new StringBuilder();
            csv.AppendLine("Bill ID,Teacher Email,Teacher Name,Year,Month,Total Meals,Meals Amount,Water Fee,Previous Due,Total Due,Paid Amount,Status,Generated On,Paid On");

            foreach (var b in bills)
            {
                csv.AppendLine($"{b.Id}," +
                    $"\"{b.Teacher?.Email ?? "N/A"}\"," +
                    $"\"{b.Teacher?.FullName ?? "N/A"}\"," +
                    $"{b.Year}," +
                    $"{b.Month}," +
                    $"{b.Lines?.Count ?? 0}," +
                    $"{b.TotalMealsAmount:F2}," +
                    $"{b.WaterFee:F2}," +
                    $"{b.PreviousDue:F2}," +
                    $"{b.TotalDue:F2}," +
                    $"{b.PaidAmount:F2}," +
                    $"\"{b.Status}\"," +
                    $"{b.GeneratedOn:yyyy-MM-dd}," +
                    $"{(b.PaidOn.HasValue ? b.PaidOn.Value.ToString("yyyy-MM-dd") : "")}");
            }

            var fileName = year.HasValue && month.HasValue 
                ? $"Bills_{year}_{month:D2}.csv" 
                : $"AllBills_{DateTime.Now:yyyyMMdd}.csv";

            return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", fileName);
        }

        // GET: Reports/AllAttendance - Download all attendance as CSV (Admin only)
        public async Task<IActionResult> AllAttendance(int? year, int? month)
        {
            if (User.Claims.FirstOrDefault(c => c.Type == "isAdmin")?.Value != "true") return Forbid();

            var query = _context.TeacherAttendances.Include(a => a.Teacher).AsQueryable();

            if (year.HasValue && month.HasValue)
            {
                var startDate = DateOnly.FromDateTime(new DateTime(year.Value, month.Value, 1));
                var endDate = startDate.AddMonths(1).AddDays(-1);
                query = query.Where(a => a.Date >= startDate && a.Date <= endDate);
            }

            var records = await query.OrderByDescending(a => a.Date).ToListAsync();

            var csv = new StringBuilder();
            csv.AppendLine("ID,Teacher Email,Teacher Name,Date,Day,Breakfast,Lunch,Dinner,Verified,Marked By,Dispute Status");

            foreach (var a in records)
            {
                csv.AppendLine($"{a.Id}," +
                    $"\"{a.Teacher?.Email ?? "N/A"}\"," +
                    $"\"{a.Teacher?.FullName ?? "N/A"}\"," +
                    $"{a.Date?.ToString("yyyy-MM-dd") ?? ""}," +
                    $"{(a.Date.HasValue ? a.Date.Value.DayOfWeek.ToString() : "")}," +
                    $"{(a.Breakfast ? "Yes" : "No")}," +
                    $"{(a.Lunch ? "Yes" : "No")}," +
                    $"{(a.Dinner ? "Yes" : "No")}," +
                    $"{(a.IsVerified == true ? "Yes" : "No")}," +
                    $"\"{a.MarkedBy ?? ""}\"," +
                    $"\"{a.DisputeStatus ?? "None"}\"");
            }

            var fileName = year.HasValue && month.HasValue 
                ? $"Attendance_{year}_{month:D2}.csv" 
                : $"AllAttendance_{DateTime.Now:yyyyMMdd}.csv";

            return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", fileName);
        }

        // GET: Reports/UsersList - Download all users as CSV (Admin only)
        public async Task<IActionResult> UsersList()
        {
            if (User.Claims.FirstOrDefault(c => c.Type == "isAdmin")?.Value != "true") return Forbid();

            var users = await _context.AspNetUsers.OrderBy(u => u.FullName).ToListAsync();

            var csv = new StringBuilder();
            csv.AppendLine("ID,Email,Full Name,Is Admin,Joined Date,Password Changed");

            foreach (var u in users)
            {
                csv.AppendLine($"\"{u.Id}\"," +
                    $"\"{u.Email ?? ""}\"," +
                    $"\"{u.FullName ?? ""}\"," +
                    $"{(u.IsAdmin == true ? "Yes" : "No")}," +
                    $"{u.JoinedDate?.ToString("yyyy-MM-dd") ?? ""}," +
                    $"{(u.IsPasswordChanged == true ? "Yes" : "No")}");
            }

            return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", $"Users_{DateTime.Now:yyyyMMdd}.csv");
        }

        // GET: Reports/FinancialSummary - Download financial summary as CSV (Admin only)
        public async Task<IActionResult> FinancialSummary(int? year)
        {
            if (User.Claims.FirstOrDefault(c => c.Type == "isAdmin")?.Value != "true") return Forbid();

            year ??= DateTime.Now.Year;

            var bills = await _context.Bills.Where(b => b.Year == year).ToListAsync();

            var csv = new StringBuilder();
            csv.AppendLine($"Financial Summary for Year {year}");
            csv.AppendLine("");
            csv.AppendLine("Month,Total Bills,Total Meals Amount,Total Water Fee,Total Due,Total Paid,Outstanding");

            for (int m = 1; m <= 12; m++)
            {
                var monthBills = bills.Where(b => b.Month == m).ToList();
                if (monthBills.Count == 0) continue;

                csv.AppendLine($"{new DateTime(year.Value, m, 1):MMMM}," +
                    $"{monthBills.Count}," +
                    $"{monthBills.Sum(b => b.TotalMealsAmount):F2}," +
                    $"{monthBills.Sum(b => b.WaterFee):F2}," +
                    $"{monthBills.Sum(b => b.TotalDue):F2}," +
                    $"{monthBills.Sum(b => b.PaidAmount):F2}," +
                    $"{monthBills.Sum(b => b.TotalDue - b.PaidAmount):F2}");
            }

            csv.AppendLine("");
            csv.AppendLine($"TOTAL,{bills.Count},{bills.Sum(b => b.TotalMealsAmount):F2},{bills.Sum(b => b.WaterFee):F2},{bills.Sum(b => b.TotalDue):F2},{bills.Sum(b => b.PaidAmount):F2},{bills.Sum(b => b.TotalDue - b.PaidAmount):F2}");

            return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", $"FinancialSummary_{year}.csv");
        }

        #endregion

        #region Teacher Reports

        // GET: Reports/MyBill - Download teacher's specific bill as CSV
        public async Task<IActionResult> MyBill(int? id)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == "userId")?.Value;
            if (string.IsNullOrEmpty(userId)) return Forbid();

            Bill? bill;
            if (id.HasValue)
            {
                bill = await _context.Bills
                    .Include(b => b.Lines)
                    .Include(b => b.Teacher)
                    .FirstOrDefaultAsync(b => b.Id == id && b.TeacherId == userId);
            }
            else
            {
                // Get latest bill
                bill = await _context.Bills
                    .Include(b => b.Lines)
                    .Include(b => b.Teacher)
                    .Where(b => b.TeacherId == userId)
                    .OrderByDescending(b => b.Year).ThenByDescending(b => b.Month)
                    .FirstOrDefaultAsync();
            }

            if (bill == null)
            {
                TempData["ErrorMessage"] = "No bill found. Bills are generated monthly by the admin. Please check back later or contact the administrator.";
                return RedirectToAction("Dashboard", "Teacher");
            }

            var csv = new StringBuilder();
            csv.AppendLine($"MONTHLY BILL - {bill.Year}/{bill.Month:D2}");
            csv.AppendLine($"Teacher: {bill.Teacher?.FullName ?? bill.Teacher?.Email ?? "N/A"}");
            csv.AppendLine($"Generated: {bill.GeneratedOn:yyyy-MM-dd}");
            csv.AppendLine($"Status: {bill.Status}");
            csv.AppendLine("");
            csv.AppendLine("MEAL DETAILS");
            csv.AppendLine("Date,Day,Meal Type,Price");

            foreach (var line in bill.Lines.OrderBy(l => l.Date))
            {
                csv.AppendLine($"{line.Date:yyyy-MM-dd},{line.Date.DayOfWeek},{line.MealType},{line.Price:F2}");
            }

            csv.AppendLine("");
            csv.AppendLine("SUMMARY");
            csv.AppendLine($"Total Meals,{bill.Lines.Count}");
            csv.AppendLine($"Meals Amount,Rs. {bill.TotalMealsAmount:F2}");
            csv.AppendLine($"Water Fee,Rs. {bill.WaterFee:F2}");
            csv.AppendLine($"Previous Due,Rs. {bill.PreviousDue:F2}");
            csv.AppendLine($"TOTAL DUE,Rs. {bill.TotalDue:F2}");
            csv.AppendLine($"Paid Amount,Rs. {bill.PaidAmount:F2}");
            csv.AppendLine($"Balance,Rs. {(bill.TotalDue - bill.PaidAmount):F2}");

            return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", $"MyBill_{bill.Year}_{bill.Month:D2}.csv");
        }

        // GET: Reports/MyAttendance - Download teacher's attendance as CSV
        public async Task<IActionResult> MyAttendance(int? year, int? month)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == "userId")?.Value;
            if (string.IsNullOrEmpty(userId)) return Forbid();

            var query = _context.TeacherAttendances.Where(a => a.TeacherId == userId);

            if (year.HasValue && month.HasValue)
            {
                var startDate = DateOnly.FromDateTime(new DateTime(year.Value, month.Value, 1));
                var endDate = startDate.AddMonths(1).AddDays(-1);
                query = query.Where(a => a.Date >= startDate && a.Date <= endDate);
            }

            var records = await query.OrderByDescending(a => a.Date).ToListAsync();
            var user = await _context.AspNetUsers.FindAsync(userId);

            var csv = new StringBuilder();
            csv.AppendLine($"ATTENDANCE RECORD - {user?.FullName ?? user?.Email ?? "Teacher"}");
            if (year.HasValue && month.HasValue)
                csv.AppendLine($"Period: {new DateTime(year.Value, month.Value, 1):MMMM yyyy}");
            csv.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm}");
            csv.AppendLine("");
            csv.AppendLine("Date,Day,Breakfast,Lunch,Dinner,Status,Dispute");

            foreach (var a in records)
            {
                csv.AppendLine($"{a.Date?.ToString("yyyy-MM-dd") ?? ""}," +
                    $"{(a.Date.HasValue ? a.Date.Value.DayOfWeek.ToString() : "")}," +
                    $"{(a.Breakfast ? "Yes" : "No")}," +
                    $"{(a.Lunch ? "Yes" : "No")}," +
                    $"{(a.Dinner ? "Yes" : "No")}," +
                    $"{(a.IsVerified == true ? "Verified" : "Pending")}," +
                    $"{a.DisputeStatus ?? "None"}");
            }

            csv.AppendLine("");
            csv.AppendLine("SUMMARY");
            csv.AppendLine($"Total Days,{records.Count}");
            csv.AppendLine($"Breakfasts,{records.Count(r => r.Breakfast)}");
            csv.AppendLine($"Lunches,{records.Count(r => r.Lunch)}");
            csv.AppendLine($"Dinners,{records.Count(r => r.Dinner)}");
            csv.AppendLine($"Verified Records,{records.Count(r => r.IsVerified == true)}");

            var fileName = year.HasValue && month.HasValue 
                ? $"MyAttendance_{year}_{month:D2}.csv" 
                : $"MyAttendance_All.csv";

            return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", fileName);
        }

        #endregion
    }
}
