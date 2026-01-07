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

            try
            {
                _logger.LogInformation("Starting bill generation for {Year}/{Month}", year, month);

                // Validate input
                if (month < 1 || month > 12)
                {
                    TempData["Error"] = "Invalid month. Please enter a value between 1 and 12.";
                    return RedirectToAction("Generate");
                }

                if (year < 2000 || year > 2100)
                {
                    TempData["Error"] = "Invalid year. Please enter a reasonable year value.";
                    return RedirectToAction("Generate");
                }

                var start = new DateTime(year, month, 1);
                var end = start.AddMonths(1).AddDays(-1);

                // Fetch users (non-admin)
                var users = await _context.AspNetUsers.Where(u => u.IsAdmin != true).ToListAsync();
                if (users == null || users.Count == 0)
                {
                    _logger.LogWarning("No non-admin users found for bill generation");
                    TempData["Error"] = "No users found to generate bills for. Please ensure there are non-admin users in the system.";
                    return RedirectToAction("Generate");
                }
                _logger.LogInformation("Found {Count} users for bill generation", users.Count);

                // Fetch weekly plans with days
                var plans = await _context.WeeklyPlans.Include(p => p.Days).ToListAsync();
                if (plans == null || plans.Count == 0)
                {
                    _logger.LogWarning("No weekly plans found - bills will be generated with zero prices");
                }
                else
                {
                    _logger.LogInformation("Found {Count} weekly plans", plans.Count);
                }
                plans ??= new List<WeeklyPlan>(); // Ensure not null

                // Fetch water fee setting
                var waterSetting = await _context.Settings.FirstOrDefaultAsync(s => s.Key == "WaterFee");
                decimal waterFee = 0m;
                if (waterSetting != null && !string.IsNullOrEmpty(waterSetting.Value))
                {
                    decimal.TryParse(waterSetting.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out waterFee);
                }
                _logger.LogInformation("Water fee: {WaterFee}", waterFee);

                var generated = new List<string>();
                var errors = new List<string>();

                foreach (var user in users)
                {
                    try
                    {
                        _logger.LogDebug("Processing user {UserId} ({Email})", user.Id, user.Email);

                        // Fetch attendances for this user with non-null dates
                        var attendances = await _context.TeacherAttendances
                            .Where(a => a.TeacherId == user.Id && a.Date != null)
                            .ToListAsync();

                        // Filter attendance for the month - with null safety
                        var monthAttendance = attendances.Where(a =>
                        {
                            if (a.Date == null || !a.Date.HasValue) return false;
                            try
                            {
                                var dt = new DateTime(a.Date.Value.Year, a.Date.Value.Month, a.Date.Value.Day);
                                return dt >= start && dt <= end;
                            }
                            catch
                            {
                                return false;
                            }
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
                            // Safe null check - should always be valid due to filter, but double-check
                            if (a.Date == null || !a.Date.HasValue) continue;

                            DateTime dt;
                            try
                            {
                                dt = new DateTime(a.Date.Value.Year, a.Date.Value.Month, a.Date.Value.Day);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogWarning(ex, "Invalid date for attendance {AttendanceId}", a.Id);
                                continue;
                            }

                            // Find plan for this date (week containing date)
                            var plan = plans.FirstOrDefault(p => p.WeekStart <= dt && p.WeekStart.AddDays(7) > dt);
                            if (plan == null)
                            {
                                plan = plans.OrderByDescending(p => p.WeekStart).FirstOrDefault();
                            }

                            WeeklyPlanDay? planDay = null;
                            if (plan != null && plan.Days != null)
                            {
                                planDay = plan.Days.FirstOrDefault(d => d.DayOfWeek == (int)dt.DayOfWeek);
                            }

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

                        // Previous due from unpaid bills - with null safety
                        try
                        {
                            var unpaidBills = await _context.Bills
                                .Where(b => b.TeacherId == user.Id && b.Status != "Paid")
                                .ToListAsync();
                            bill.PreviousDue = unpaidBills.Sum(b => b.TotalDue - b.PaidAmount);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Could not calculate previous due for user {UserId}", user.Id);
                            bill.PreviousDue = 0m;
                        }

                        bill.TotalDue = bill.TotalMealsAmount + bill.WaterFee + bill.PreviousDue;
                        bill.PaidAmount = 0m;

                        _context.Bills.Add(bill);
                        await _context.SaveChangesAsync();

                        generated.Add($"{user.Email ?? user.Id}: bill #{bill.Id}, {bill.Lines.Count} meals, total {bill.TotalDue:C}");
                        _logger.LogInformation("Generated bill {BillId} for user {UserId} with {LineCount} lines, total {Total}",
                            bill.Id, user.Id, bill.Lines.Count, bill.TotalDue);
                    }
                    catch (Exception userEx)
                    {
                        _logger.LogError(userEx, "Error generating bill for user {UserId} ({Email})", user.Id, user.Email);
                        errors.Add($"Error for {user.Email ?? user.Id}: {userEx.Message}");
                    }
                }

                // Build result message
                var resultMessage = new List<string>();
                if (generated.Count > 0)
                {
                    resultMessage.Add($"Successfully generated {generated.Count} bill(s):");
                    resultMessage.AddRange(generated);
                }
                if (errors.Count > 0)
                {
                    resultMessage.Add($"\n{errors.Count} error(s) occurred:");
                    resultMessage.AddRange(errors);
                }

                if (generated.Count == 0 && errors.Count == 0)
                {
                    TempData["GeneratedReport"] = "No bills were generated. This may be because there is no attendance data for the selected month.";
                    TempData["ErrorMessage"] = "No bills generated - no attendance data found for the selected period.";
                }
                else if (generated.Count > 0)
                {
                    TempData["GeneratedReport"] = string.Join("\n", resultMessage);
                    TempData["SuccessMessage"] = $"✓ Successfully generated {generated.Count} bill(s) for {year}/{month:D2}!";
                }
                else
                {
                    TempData["GeneratedReport"] = string.Join("\n", resultMessage);
                }

                if (errors.Count > 0 && generated.Count == 0)
                {
                    TempData["ErrorMessage"] = "Bill generation failed. Check the report for details.";
                }

                return RedirectToAction("Index", "Bills");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Critical error during bill generation for {Year}/{Month}", year, month);
                TempData["ErrorMessage"] = $"An error occurred while generating bills: {ex.Message}. Please check that all required data (users, weekly plans, settings) exists in the database.";
                return RedirectToAction("Generate");
            }
        }
    }
}