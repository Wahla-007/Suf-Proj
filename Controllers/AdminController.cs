using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using mess_management.Models;

namespace mess_management.Controllers
{
    [Authorize]
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;

        public AdminController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            if (User.Claims.FirstOrDefault(c => c.Type == "isAdmin")?.Value != "true") return Forbid();

            var vm = new AdminDashboardViewModel
            {
                UserCount = await _context.AspNetUsers.CountAsync(u => u.IsAdmin != true),
                WeeklyPlanCount = await _context.WeeklyPlans.CountAsync(),
                WeeklyMenuCount = await _context.WeeklyMenus.CountAsync(),
                AttendanceCount = await _context.TeacherAttendances.CountAsync(),
                BillCount = await _context.Bills.CountAsync()
            };

            return View(vm);
        }

        // GET: Admin/SeedSampleAndGenerate
        public async Task<IActionResult> SeedSampleAndGenerate(string userEmail = "user1@local", int? year = null, int? month = null)
        {
            if (User.Claims.FirstOrDefault(c => c.Type == "isAdmin")?.Value != "true") return Forbid();

            year ??= DateTime.Now.Year;
            month ??= DateTime.Now.Month;

            var user = await _context.AspNetUsers.FirstOrDefaultAsync(u => u.Email == userEmail);
            if (user == null) return NotFound($"User {userEmail} not found.");

            // Create a weekly plan for the first week of the month with distinct meal names
            var start = new DateTime(year.Value, month.Value, 1);
            // find Monday of that week
            var monday = start;
            while (monday.DayOfWeek != DayOfWeek.Monday) monday = monday.AddDays(-1);

            var plan = new WeeklyPlan
            {
                WeekStart = monday,
                CreatedAt = DateTime.Now,
                CreatedById = User.Claims.FirstOrDefault(c => c.Type == "userId")?.Value
            };
            plan.Days = new List<WeeklyPlanDay>();
            var breakfastNames = new[] { "Continental", "Paratha Special", "Porridge", "Omelette", "Aloo Paratha", "Toast & Jam", "Pancakes" };
            var lunchNames = new[] { "Rice & Curry", "Biryani", "Dal Chawal", "Chicken Curry", "Veg Thali", "Spaghetti", "Grilled Veg" };
            var dinnerNames = new[] { "Roti & Sabzi", "Kebab Night", "Soup & Salad", "Paneer Delight", "Fish Curry", "Pizza Slice", "Mixed Rice" };

            for (int d = 0; d < 7; d++)
            {
                plan.Days.Add(new WeeklyPlanDay
                {
                    DayOfWeek = d,
                    BreakfastName = breakfastNames[d],
                    LunchName = lunchNames[d],
                    DinnerName = dinnerNames[d],
                    BreakfastPrice = 25m + d,
                    LunchPrice = 60m + d * 2,
                    DinnerPrice = 40m + d
                });
            }

            _context.WeeklyPlans.Add(plan);
            await _context.SaveChangesAsync();

            // seed attendance for user for first 5 days of month
            var attendances = new List<TeacherAttendance>();
            for (int i = 1; i <= Math.Min(5, DateTime.DaysInMonth(year.Value, month.Value)); i++)
            {
                attendances.Add(new TeacherAttendance
                {
                    TeacherId = user.Id,
                    Date = DateOnly.FromDateTime(new DateTime(year.Value, month.Value, i)),
                    Breakfast = true,
                    Lunch = true,
                    Dinner = false,
                    MarkedBy = User.Identity?.Name ?? "Admin",
                    IsVerified = true,
                    VerifiedAt = DateTime.Now
                });
            }
            _context.TeacherAttendances.AddRange(attendances);
            await _context.SaveChangesAsync();

            // Generate bill for this user for the month
            var startDate = new DateTime(year.Value, month.Value, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            var userAttendances = await _context.TeacherAttendances
                .Where(a => a.TeacherId == user.Id && a.Date != null)
                .ToListAsync();

            userAttendances = userAttendances.Where(a =>
            {
                var dt = new DateTime(a.Date.Value.Year, a.Date.Value.Month, a.Date.Value.Day);
                return dt >= startDate && dt <= endDate;
            }).ToList();

            var plans = await _context.WeeklyPlans.Include(p => p.Days).ToListAsync();
            var waterSetting = await _context.Settings.FirstOrDefaultAsync(s => s.Key == "WaterFee");
            decimal waterFee = 0m;
            if (waterSetting != null) decimal.TryParse(waterSetting.Value, out waterFee);

            var bill = new Bill
            {
                TeacherId = user.Id,
                Year = year.Value,
                Month = month.Value,
                GeneratedOn = DateTime.Now,
                Status = "Pending",
                Lines = new List<BillLine>()
            };

            foreach (var a in userAttendances)
            {
                var dt = new DateTime(a.Date.Value.Year, a.Date.Value.Month, a.Date.Value.Day);
                var p = plans.FirstOrDefault(pp => pp.WeekStart <= dt && pp.WeekStart.AddDays(7) > dt) ?? plans.OrderByDescending(pp => pp.WeekStart).FirstOrDefault();
                var planDay = p?.Days.FirstOrDefault(d => d.DayOfWeek == (int)dt.DayOfWeek);

                if (a.Breakfast == true)
                {
                    var price = planDay?.BreakfastPrice ?? 0m;
                    var name = planDay?.BreakfastName ?? "Breakfast";
                    bill.Lines.Add(new BillLine { Date = dt, MealType = name, Price = price, IsVerified = true });
                }
                if (a.Lunch == true)
                {
                    var price = planDay?.LunchPrice ?? 0m;
                    var name = planDay?.LunchName ?? "Lunch";
                    bill.Lines.Add(new BillLine { Date = dt, MealType = name, Price = price, IsVerified = true });
                }
                if (a.Dinner == true)
                {
                    var price = planDay?.DinnerPrice ?? 0m;
                    var name = planDay?.DinnerName ?? "Dinner";
                    bill.Lines.Add(new BillLine { Date = dt, MealType = name, Price = price, IsVerified = true });
                }
            }

            bill.TotalMealsAmount = bill.Lines.Sum(l => l.Price);
            bill.WaterFee = waterFee;
            bill.PreviousDue = await _context.Bills.Where(b => b.TeacherId == user.Id && b.Status != "Paid").SumAsync(b => b.TotalDue - b.PaidAmount);
            bill.TotalDue = bill.TotalMealsAmount + bill.WaterFee + bill.PreviousDue;
            bill.PaidAmount = 0m;

            _context.Bills.Add(bill);
            await _context.SaveChangesAsync();

            return Content($"Generated bill {bill.Id} for {user.Email} for {year}/{month}. Lines: {bill.Lines.Count}. TotalDue: {bill.TotalDue}");
        }
    }

    public class AdminDashboardViewModel
    {
        public int UserCount { get; set; }
        public int WeeklyPlanCount { get; set; }
        public int WeeklyMenuCount { get; set; }
        public int AttendanceCount { get; set; }
        public int BillCount { get; set; }
    }
}