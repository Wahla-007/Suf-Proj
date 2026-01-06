using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using mess_management.Models;

namespace mess_management.Controllers
{
    [Authorize]
    public class TeacherController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ILogger<TeacherController> _logger;

        public TeacherController(AppDbContext context, ILogger<TeacherController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Teacher Dashboard
        public async Task<IActionResult> Dashboard()
        {
            try
            {
                // Get current user ID from claims
                var userId = User.Claims.FirstOrDefault(c => c.Type == "userId")?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("Dashboard", "Teacher") });
                }

                var teacher = await _context.AspNetUsers.FindAsync(userId);
                if (teacher == null)
                {
                    _logger.LogWarning($"Teacher record not found for userId={userId}");
                    return View("MissingTeacher"); // show a friendly view
                }

                // Get teacher's attendance records
                var attendanceRecords = await _context.TeacherAttendances
                    .Where(ta => ta.TeacherId == userId)
                    .OrderByDescending(ta => ta.Date)
                    .Take(30)
                    .ToListAsync();

                // Get current weekly plan
                var currentPlan = await _context.WeeklyPlans
                    .Include(wp => wp.Days)
                    .OrderByDescending(wp => wp.WeekStart)
                    .FirstOrDefaultAsync();

                // Get monthly bills
                var monthlyBills = await _context.MonthlyBills
                    .Where(mb => mb.TeacherId == userId)
                    .OrderByDescending(mb => mb.GeneratedOn)
                    .Take(12)
                    .ToListAsync();

                var activeCharges = monthlyBills
                    .Where(b => b.Status != "Paid")
                    .Sum(b => b.TotalDue ?? 0);

                var viewModel = new TeacherDashboardViewModel
                {
                    Teacher = teacher,
                    AttendanceRecords = attendanceRecords,
                    CurrentPlan = currentPlan,
                    MonthlyBills = monthlyBills,
                    TotalAttendance = attendanceRecords.Count,
                    VerifiedRecords = attendanceRecords.Count(a => a.IsVerified == true),
                    PendingRecords = attendanceRecords.Count(a => a.IsVerified != true),
                    ActiveCharges = activeCharges
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error loading teacher dashboard: {ex.Message}");
                return View(null);
            }
        }

        // GET: Teacher Attendance History
        public async Task<IActionResult> AttendanceHistory()
        {
            try
            {
                var userId = User.Claims.FirstOrDefault(c => c.Type == "userId")?.Value;
                if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("AttendanceHistory", "Teacher") });
                
                var attendanceRecords = await _context.TeacherAttendances
                    .Where(ta => ta.TeacherId == userId)
                    .OrderByDescending(ta => ta.Date)
                    .ToListAsync();

                return View(attendanceRecords);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error loading attendance history: {ex.Message}");
                return View(new List<TeacherAttendance>());
            }
        }

        // GET: Teacher Bills
        public async Task<IActionResult> MyBills()
        {
            try
            {
                var userId = User.Claims.FirstOrDefault(c => c.Type == "userId")?.Value;
                if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("MyBills", "Teacher") });
                
                var bills = await _context.MonthlyBills
                    .Where(mb => mb.TeacherId == userId)
                    .OrderByDescending(mb => mb.GeneratedOn)
                    .ToListAsync();

                return View(bills);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error loading bills: {ex.Message}");
                return View(new List<MonthlyBill>());
            }
        }

        // GET: Weekly Menu
        public async Task<IActionResult> ViewMenu()
        {
            try
            {
                // Fetch the latest weekly plan
                var currentPlan = await _context.WeeklyPlans
                    .Include(wp => wp.Days)
                    .Include(wp => wp.CreatedBy)
                    .OrderByDescending(wp => wp.WeekStart)
                    .FirstOrDefaultAsync();

                return View(currentPlan);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error loading menu: {ex.Message}");
                return View(null);
            }
        }

        // GET: Teacher/RaiseDispute/5
        public async Task<IActionResult> RaiseDispute(int id)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == "userId")?.Value;
            var record = await _context.TeacherAttendances
                .FirstOrDefaultAsync(ta => ta.Id == id && ta.TeacherId == userId);

            if (record == null) return NotFound();
            if (record.DisputeStatus != "None" && !string.IsNullOrEmpty(record.DisputeStatus))
            {
                return RedirectToAction(nameof(AttendanceHistory));
            }

            return View(record);
        }

        // POST: Teacher/RaiseDispute
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RaiseDispute(int id, string reason)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == "userId")?.Value;
            var record = await _context.TeacherAttendances
                .FirstOrDefaultAsync(ta => ta.Id == id && ta.TeacherId == userId);

            if (record == null) return NotFound();

            record.DisputeStatus = "Raised";
            record.DisputeReason = reason;
            
            _context.Update(record);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Dispute raised successfully. Admin will review it.";
            return RedirectToAction(nameof(AttendanceHistory));
        }

        // POST: Teacher/MarkMealTaken
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkMealTaken(string mealType)
        {
            // ... (keep existing logic for fallback) ...
            return await ToggleMealLogic(mealType, false);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleMealAjax([FromBody] MealToggleRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.MealType)) return BadRequest(new { success = false, message = "Invalid request" });
            return await ToggleMealLogic(request.MealType, true);
        }

        private async Task<IActionResult> ToggleMealLogic(string mealType, bool isAjax)
        {
            try
            {
                var userId = User.Claims.FirstOrDefault(c => c.Type == "userId")?.Value;
                var userEmail = User.Claims.FirstOrDefault(c => c.Type == "email")?.Value;

                if (string.IsNullOrEmpty(userId))
                {
                    if (isAjax) return Unauthorized(new { success = false, message = "Unauthorized" });
                    return RedirectToAction("Login", "Account");
                }

                var today = DateOnly.FromDateTime(DateTime.Today);

                // Check if attendance record for today already exists
                var existingRecord = await _context.TeacherAttendances
                    .FirstOrDefaultAsync(ta => ta.TeacherId == userId && ta.Date == today);

                bool isTaken = false;

                if (existingRecord != null)
                {
                    // Toggle the specific meal field
                    switch (mealType.ToLower())
                    {
						case "breakfast":
                            existingRecord.Breakfast = !existingRecord.Breakfast;
                            isTaken = existingRecord.Breakfast;
                            break;
                        case "lunch":
                            existingRecord.Lunch = !existingRecord.Lunch;
                            isTaken = existingRecord.Lunch;
                            break;
                        case "dinner":
                            existingRecord.Dinner = !existingRecord.Dinner;
                            isTaken = existingRecord.Dinner;
                            break;
                        default:
                            if (isAjax) return BadRequest(new { success = false, message = "Invalid meal type" });
                            TempData["ErrorMessage"] = "Invalid meal type!";
                            return RedirectToAction(nameof(Dashboard));
                    }

                    _context.Update(existingRecord);
                }
                else
                {
                    // Create new attendance record
                    var newRecord = new TeacherAttendance
                    {
                        TeacherId = userId,
                        Date = today,
                        MarkedBy = userEmail ?? userId,
                        IsVerified = null // Pending verification
                    };

                    // Set the specific meal (others default to null)
                    switch (mealType.ToLower())
                    {
                        case "breakfast": newRecord.Breakfast = true; isTaken = true; break;
                        case "lunch": newRecord.Lunch = true; isTaken = true; break;
                        case "dinner": newRecord.Dinner = true; isTaken = true; break;
                    }

                    _context.TeacherAttendances.Add(newRecord);
                }

                await _context.SaveChangesAsync();

                string statusMessage = isTaken ? "Confirmed" : "Skipped";
                
                if (isAjax)
                {
                    return Ok(new { success = true, isTaken = isTaken, message = $"{mealType} {statusMessage}" });
                }

                TempData["SuccessMessage"] = $"{mealType} {statusMessage} successfully!";
                return RedirectToAction(nameof(Dashboard));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error marking meal: {ex.Message}");
                if (isAjax) return StatusCode(500, new { success = false, message = "Server error" });
                TempData["ErrorMessage"] = "An error occurred while marking the meal.";
                return RedirectToAction(nameof(Dashboard));
            }
        }

        // GET: Teacher/PrintChallan/5
        public async Task<IActionResult> PrintChallan(int id)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == "userId")?.Value;
            var bill = await _context.MonthlyBills
                .Include(b => b.Teacher)
                .FirstOrDefaultAsync(b => b.Id == id && b.TeacherId == userId);

            if (bill == null) return NotFound();

            return View("Challan", bill);
        }


    }


}
