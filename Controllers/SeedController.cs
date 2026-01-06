using Microsoft.AspNetCore.Mvc;
using mess_management.Models;
using Microsoft.EntityFrameworkCore;

namespace mess_management.Controllers
{
    public class SeedController : Controller
    {
        private readonly AppDbContext _context;

        public SeedController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> SeedDisputes()
        {
            var users = await _context.AspNetUsers.Where(u => u.IsAdmin != true).Take(3).ToListAsync();
            if (!users.Any()) return Content("No teachers found to seed disputes.");

            var reasons = new[] {
                "I was on leave this day, please check my application.",
                "I didn't have lunch, but I'm charged for it.",
                "Double entry for the same day.",
                "Incorrect meal type marked.",
                "I was out of station."
            };

            int added = 0;
            foreach (var user in users)
            {
                for (int i = 0; i < 2; i++)
                {
                    var date = DateOnly.FromDateTime(DateTime.Now.AddDays(-(i + 2 * (added + 1))));
                    
                    // Check if already exists to avoid duplicates
                    var exists = await _context.TeacherAttendances.AnyAsync(a => a.TeacherId == user.Id && a.Date == date);
                    if (exists) continue;

                    _context.TeacherAttendances.Add(new TeacherAttendance
                    {
                        TeacherId = user.Id,
                        Date = date,
                        Breakfast = true,
                        Lunch = true,
                        Dinner = false,
                        MarkedBy = "Staff",
                        DisputeStatus = "Raised",
                        DisputeReason = reasons[(added) % reasons.Length],
                        IsVerified = true
                    });
                    added++;
                }
            }

            await _context.SaveChangesAsync();
            return Content($"Seeded {added} sample disputes for {users.Count} users. Go to /TeacherAttendance/Disputes to view them.");
        }

        public async Task<IActionResult> SeedAll()
        {
            var users = await _context.AspNetUsers.ToListAsync();
            var random = new Random();
            int recordsAdded = 0;

            foreach (var user in users)
            {
                // Check and Add Attendance
                var attendanceCount = await _context.TeacherAttendances.CountAsync(t => t.TeacherId == user.Id);
                if (attendanceCount < 4)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        var date = DateOnly.FromDateTime(DateTime.Now.AddDays(-(i + 1)));
                        _context.TeacherAttendances.Add(new TeacherAttendance
                        {
                            TeacherId = user.Id,
                            Date = date,
                            Breakfast = true,
                            Lunch = true,
                            Dinner = i % 2 == 0, // Alternate dinner
                            IsVerified = i != 0, // Latest pending, others verified
                            VerifiedAt = i != 0 ? DateTime.Now : null,
                            DisputeStatus = "None"
                        });
                        recordsAdded++;
                    }
                }

                // Check and Add Bills
                var billsCount = await _context.MonthlyBills.CountAsync(b => b.TeacherId == user.Id);
                if (billsCount < 4)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        var billDate = DateTime.Now.AddMonths(-(i + 1));
                        decimal foodAmt = random.Next(2000, 4000);
                        decimal waterShare = 150;
                        decimal totalDue = foodAmt + waterShare;
                        
                        _context.MonthlyBills.Add(new MonthlyBill
                        {
                            TeacherId = user.Id,
                            Year = billDate.Year,
                            Month = billDate.Month,
                            TotalMeals = random.Next(20, 60),
                            FoodAmount = foodAmt,
                            WaterShare = waterShare,
                            TotalDue = totalDue,
                            PaidAmount = i > 1 ? totalDue : 0, // Older 2 paid, newer 2 unpaid
                            Status = i > 1 ? "Paid" : "Unpaid",
                            GeneratedOn = billDate,
                            PaidOn = i > 1 ? DateTime.Now : null
                        });
                        recordsAdded++;
                    }
                }
            }

            await _context.SaveChangesAsync();
            return Content($"Seeding Complete! Added {recordsAdded} records across {users.Count} users.");
        }
        public async Task<IActionResult> SeedDemoUiData()
        {
            var users = await _context.AspNetUsers.Take(3).ToListAsync();
            if (!users.Any()) return Content("No users found.");

            var demoRecords = new List<TeacherAttendance>();
            var today = DateOnly.FromDateTime(DateTime.Now);
            var user = users.First(); 
            var user2 = users.Count > 1 ? users[1] : user;

            // 1. Verified - All Good
            demoRecords.Add(new TeacherAttendance {
                TeacherId = user.Id, Date = today.AddDays(-1),
                Breakfast = true, Lunch = true, Dinner = true,
                MarkedBy = "System", IsVerified = true, VerificationNote = "Verified daily log.", VerifiedAt = DateTime.Now
            });

            // 2. Rejected - Duplicate
            demoRecords.Add(new TeacherAttendance {
                TeacherId = user.Id, Date = today.AddDays(-2),
                Breakfast = true, Lunch = false, Dinner = false,
                MarkedBy = "Admin", IsVerified = false, VerificationNote = "Duplicate entry detected.", VerifiedAt = DateTime.Now
            });

            // 3. Dispute Raised
            demoRecords.Add(new TeacherAttendance {
                TeacherId = user.Id, Date = today.AddDays(-3),
                Breakfast = false, Lunch = true, Dinner = true,
                MarkedBy = "System", IsVerified = null, DisputeStatus = "Raised", DisputeReason = "I was absent for Lunch."
            });

            // 4. Dispute Approved (Admin accepted dispute)
            demoRecords.Add(new TeacherAttendance {
                TeacherId = user.Id, Date = today.AddDays(-4),
                Breakfast = true, Lunch = true, Dinner = true,
                MarkedBy = "System", IsVerified = true, DisputeStatus = "Approved", DisputeReason = "Marked absent incorrectly.", VerificationNote = "Dispute Valid. Updated."
            });

            // 5. Dispute Rejected
            demoRecords.Add(new TeacherAttendance {
                TeacherId = user.Id, Date = today.AddDays(-5),
                Breakfast = true, Lunch = true, Dinner = false,
                MarkedBy = "System", IsVerified = true, DisputeStatus = "Rejected", DisputeReason = "I did not eat breakfast.", VerificationNote = "CCTV confirms attendance."
            });
            
             // 6. Pending Review
            demoRecords.Add(new TeacherAttendance {
                TeacherId = user2.Id, Date = today.AddDays(-1),
                Breakfast = true, Lunch = false, Dinner = true,
                MarkedBy = "System", IsVerified = null, VerificationNote = null
            });
            
             // 7. Another Verified
            demoRecords.Add(new TeacherAttendance {
                TeacherId = user2.Id, Date = today.AddDays(-2),
                Breakfast = false, Lunch = true, Dinner = false,
                MarkedBy = "Staff", IsVerified = true, VerificationNote = "Manual Entry"
            });

            _context.TeacherAttendances.AddRange(demoRecords);
            await _context.SaveChangesAsync();

            return Content("Added 7 demo records with various statuses for UI testing.");
        }
    }
}
