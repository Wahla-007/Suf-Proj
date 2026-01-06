using System;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using mess_management.Models;

namespace mess_management.Controllers
{
    [Authorize]
    public class WeeklyPlansController : Controller
    {
        private readonly AppDbContext _context;

        public WeeklyPlansController(AppDbContext context)
        {
            _context = context;
        }

        // GET: WeeklyPlans
        public async Task<IActionResult> Index()
        {
            bool isAdmin = false;
            foreach (var claim in User.Claims)
            {
                if (claim.Type == "isAdmin" && claim.Value == "true")
                {
                    isAdmin = true;
                    break;
                }
            }
            if (!isAdmin) return Forbid();

            var plans = await _context.WeeklyPlans
                .Include(p => p.Days)
                .Include(p => p.CreatedBy)
                .ToListAsync();
            return View(plans);
        }

        // GET: WeeklyPlans/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            bool isAdmin = false;
            foreach (var claim in User.Claims)
            {
                if (claim.Type == "isAdmin" && claim.Value == "true")
                {
                    isAdmin = true;
                    break;
                }
            }
            if (!isAdmin) return Forbid();

            if (id == null) return NotFound();
            var plan = await _context.WeeklyPlans.Include(p => p.Days).Include(p => p.CreatedBy).FirstOrDefaultAsync(p => p.Id == id);
            if (plan == null) return NotFound();
            
            // Ensure there are 7 days with proper defaults
            var daysList = new List<WeeklyPlanDay>();
            for (int i = 0; i < 7; i++)
            {
                WeeklyPlanDay day = null;
                foreach (var d in plan.Days)
                {
                    if (d.DayOfWeek == i)
                    {
                        day = d;
                        break;
                    }
                }

                if (day != null)
                {
                    daysList.Add(day);
                }
                else
                {
                    daysList.Add(new WeeklyPlanDay
                    {
                        DayOfWeek = i,
                        BreakfastName = "Breakfast",
                        BreakfastPrice = 100,
                        LunchName = "Lunch",
                        LunchPrice = 150,
                        DinnerName = "Dinner",
                        DinnerPrice = 120
                    });
                }
            }
            plan.Days = daysList;
            
            return View(plan);
        }

        // POST: WeeklyPlans/Details/5
        [HttpPost]
        [ActionName("Details")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateDetails(int id)
        {
            bool isAdmin = false;
            foreach (var claim in User.Claims)
            {
                if (claim.Type == "isAdmin" && claim.Value == "true")
                {
                    isAdmin = true;
                    break;
                }
            }
            if (!isAdmin) return Forbid();

            try
            {
                WeeklyPlan existing = await _context.WeeklyPlans.Include(p => p.Days).FirstOrDefaultAsync(p => p.Id == id);
                if (existing == null) return NotFound();

                var form = await Request.ReadFormAsync();
                
                // Update week start
                if (form.ContainsKey("WeekStart"))
                {
                    string ws = form["WeekStart"].ToString();
                    if (DateTime.TryParse(ws, out DateTime weekStart))
                    {
                        existing.WeekStart = weekStart;
                    }
                }
                
                for (int i = 0; i < 7; i++)
                {
                    string dayOfWeekStr = form[$"Days[{i}].DayOfWeek"].ToString();
                    if (string.IsNullOrEmpty(dayOfWeekStr)) continue;
                    
                    if (int.TryParse(dayOfWeekStr, out int dayOfWeek))
                    {
                        WeeklyPlanDay existingDay = null;
                        foreach (var d in existing.Days)
                        {
                            if (d.DayOfWeek == dayOfWeek)
                            {
                                existingDay = d;
                                break;
                            }
                        }
                        
                        if (existingDay != null)
                        {
                            existingDay.BreakfastName = form[$"Days[{i}].BreakfastName"].ToString();
                            if (decimal.TryParse(form[$"Days[{i}].BreakfastPrice"].ToString(), out decimal bp)) 
                                existingDay.BreakfastPrice = bp;
                            
                            existingDay.LunchName = form[$"Days[{i}].LunchName"].ToString();
                            if (decimal.TryParse(form[$"Days[{i}].LunchPrice"].ToString(), out decimal lp)) 
                                existingDay.LunchPrice = lp;
                            
                            existingDay.DinnerName = form[$"Days[{i}].DinnerName"].ToString();
                            if (decimal.TryParse(form[$"Days[{i}].DinnerPrice"].ToString(), out decimal dp)) 
                                existingDay.DinnerPrice = dp;
                        }
                    }
                }
                
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Changes saved successfully!";
                return RedirectToAction("Details", new { id = id });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        // GET: WeeklyPlans/Create
        public IActionResult Create()
        {
            if (User.Claims.FirstOrDefault(c => c.Type == "isAdmin")?.Value != "true") return Forbid();

            var model = new WeeklyPlan();
            // Default to next Monday
            var nextMonday = DateTime.Today;
            while (nextMonday.DayOfWeek != DayOfWeek.Monday) nextMonday = nextMonday.AddDays(1);
            model.WeekStart = nextMonday;

            for (int i = 0; i < 7; i++)
            {
                model.Days.Add(new WeeklyPlanDay { DayOfWeek = i });
            }
            return View(model);
        }

        // POST: WeeklyPlans/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(IFormCollection form)
        {
            if (User.Claims.FirstOrDefault(c => c.Type == "isAdmin")?.Value != "true") return Forbid();

            try
            {
                var weekStartStr = form["WeekStart"];
                DateTime weekStart = DateTime.Parse(weekStartStr);
                string userId = User.Claims.FirstOrDefault(c => c.Type == "userId")?.Value;

                var newPlan = new WeeklyPlan
                {
                    WeekStart = weekStart,
                    CreatedAt = DateTime.Now,
                    CreatedById = userId,
                    Days = new List<WeeklyPlanDay>()
                };

                for (int i = 0; i < 7; i++)
                {
                    var day = new WeeklyPlanDay
                    {
                        DayOfWeek = int.Parse(form[$"Days[{i}].DayOfWeek"]),
                        BreakfastName = form[$"Days[{i}].BreakfastName"],
                        BreakfastPrice = decimal.TryParse(form[$"Days[{i}].BreakfastPrice"], out var bp) ? bp : 0m,
                        LunchName = form[$"Days[{i}].LunchName"],
                        LunchPrice = decimal.TryParse(form[$"Days[{i}].LunchPrice"], out var lp) ? lp : 0m,
                        DinnerName = form[$"Days[{i}].DinnerName"],
                        DinnerPrice = decimal.TryParse(form[$"Days[{i}].DinnerPrice"], out var dp) ? dp : 0m
                    };
                    newPlan.Days.Add(day);
                }

                _context.Add(newPlan);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Weekly plan created successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error creating plan: " + ex.Message;
                // If it fails, we should really return to the view with the entered data, 
                // but since we're using IFormCollection, we'll just redirect to Index with error for now 
                // or we could rebuild the model. For simplicity and robustness, redirecting works well here.
                return RedirectToAction(nameof(Index));
            }
        }



        // GET: WeeklyPlans/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            bool isAdmin = false;
            foreach (var claim in User.Claims)
            {
                if (claim.Type == "isAdmin" && claim.Value == "true")
                {
                    isAdmin = true;
                    break;
                }
            }
            if (!isAdmin) return Forbid();

            if (id == null) return NotFound();
            var plan = await _context.WeeklyPlans.Include(p => p.Days).FirstOrDefaultAsync(p => p.Id == id);
            if (plan == null) return NotFound();
            
            // Ensure there are 7 days
            var daysList = new List<WeeklyPlanDay>();
            for (int i = 0; i < 7; i++)
            {
                WeeklyPlanDay day = null;
                foreach (var d in plan.Days)
                {
                    if (d.DayOfWeek == i)
                    {
                        day = d;
                        break;
                    }
                }
                if (day != null) daysList.Add(day);
                else daysList.Add(new WeeklyPlanDay { DayOfWeek = i });
            }
            plan.Days = daysList;
            return View(plan);
        }

        // POST: WeeklyPlans/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, WeeklyPlan model)
        {
            bool isAdmin = false;
            foreach (var claim in User.Claims)
            {
                if (claim.Type == "isAdmin" && claim.Value == "true")
                {
                    isAdmin = true;
                    break;
                }
            }
            if (!isAdmin) return Forbid();

            if (id != model.Id) return NotFound();
            if (!ModelState.IsValid) return View(model);

            var existing = await _context.WeeklyPlans.Include(p => p.Days).FirstOrDefaultAsync(p => p.Id == id);
            if (existing == null) return NotFound();

            existing.WeekStart = model.WeekStart;
            // sync days
            _context.WeeklyPlanDays.RemoveRange(existing.Days);
            existing.Days = model.Days;
            _context.Update(existing);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        // GET: WeeklyPlans/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            bool isAdmin = false;
            foreach (var claim in User.Claims)
            {
                if (claim.Type == "isAdmin" && claim.Value == "true")
                {
                    isAdmin = true;
                    break;
                }
            }
            if (!isAdmin) return Forbid();

            if (id == null) return NotFound();
            var plan = await _context.WeeklyPlans.Include(p => p.Days).FirstOrDefaultAsync(p => p.Id == id);
            if (plan == null) return NotFound();
            return View(plan);
        }

        // POST: WeeklyPlans/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            bool isAdmin = false;
            foreach (var claim in User.Claims)
            {
                if (claim.Type == "isAdmin" && claim.Value == "true")
                {
                    isAdmin = true;
                    break;
                }
            }
            if (!isAdmin) return Forbid();

            var plan = await _context.WeeklyPlans.FindAsync(id);
            if (plan != null)
            {
                _context.WeeklyPlans.Remove(plan);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }
    }
}