using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using mess_management.Models;

namespace mess_management.Controllers
{
    [Authorize]
    public class BillingController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ILogger<BillingController> _logger;

        public BillingController(AppDbContext context, ILogger<BillingController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public IActionResult Generate()
        {
            if (User.Claims.FirstOrDefault(c => c.Type == "isAdmin")?.Value != "true") return Forbid();
            ViewData["Year"] = DateTime.Now.Year;
            ViewData["Month"] = DateTime.Now.Month;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Generate(int year, int month)
        {
            if (User.Claims.FirstOrDefault(c => c.Type == "isAdmin")?.Value != "true") return Forbid();
            var start = new DateTime(year, month, 1);
            var end = start.AddMonths(1).AddDays(-1);

            var users = await _context.AspNetUsers.Where(u => u.IsAdmin != true).ToListAsync();
            var plans = await _context.WeeklyPlans.Include(p => p.Days).ToListAsync();
            var waterSetting = await _context.Settings.FirstOrDefaultAsync(s => s.Key == "WaterFee");
            decimal waterFee = 0m;
            if (waterSetting != null) decimal.TryParse(waterSetting.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out waterFee);

            var generated = new List<string>();

            foreach (var user in users)
            {
                var attendances = await _context.TeacherAttendances
                    .Where(a => a.TeacherId == user.Id && a.Date != null)
                    .ToListAsync();

                // filter attendance for the month
                var monthAttendance = attendances.Where(a =>
                {
                    // Date is DateOnly in model
                    var dt = new DateTime(a.Date.Value.Year, a.Date.Value.Month, a.Date.Value.Day);
                    return dt >= start && dt <= end;
                }).ToList();

                var bill = new Bill
                {
                    TeacherId = user.Id,
                    Year = year,
                    Month = month,
                    GeneratedOn = DateTime.Now,
                    Status = "Pending",
                    Lines = new List<BillLine>()
                };

                foreach (var a in monthAttendance)
                {
                    var dt = new DateTime(a.Date.Value.Year, a.Date.Value.Month, a.Date.Value.Day);
                    // find plan for this date (week containing date)
                    var plan = plans.FirstOrDefault(p => p.WeekStart <= dt && p.WeekStart.AddDays(7) > dt);
                    if (plan == null) plan = plans.OrderByDescending(p => p.WeekStart).FirstOrDefault();
                    WeeklyPlanDay? planDay = null;
                    if (plan != null) planDay = plan.Days.FirstOrDefault(d => d.DayOfWeek == (int)dt.DayOfWeek);

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
                // previous due from unpaid bills
                bill.PreviousDue = await _context.Bills.Where(b => b.TeacherId == user.Id && b.Status != "Paid").SumAsync(b => b.TotalDue - b.PaidAmount);
                bill.TotalDue = bill.TotalMealsAmount + bill.WaterFee + bill.PreviousDue;
                bill.PaidAmount = 0m;

                _context.Bills.Add(bill);
                await _context.SaveChangesAsync();
                generated.Add($"{user.Email}: bill {bill.Id} lines {bill.Lines.Count} total {bill.TotalDue}");
            }

            TempData["GeneratedReport"] = string.Join("\n", generated);
            return RedirectToAction("Index", "Bills");
        }
    }
}