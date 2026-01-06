using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using mess_management.Models;

namespace mess_management.Controllers
{
    [Authorize]
    public class WeeklyMenuController : Controller
    {
        private readonly AppDbContext _context;

        public WeeklyMenuController(AppDbContext context)
        {
            _context = context;
        }

        // GET: WeeklyMenu
        public async Task<IActionResult> Index()
        {
            if (User.Claims.FirstOrDefault(c => c.Type == "isAdmin")?.Value != "true") return Forbid();
            var appDbContext = _context.WeeklyMenus.Include(w => w.CreatedBy);
            return View(await appDbContext.ToListAsync());
        }

        // GET: WeeklyMenu/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (User.Claims.FirstOrDefault(c => c.Type == "isAdmin")?.Value != "true") return Forbid();
            if (id == null)
            {
                return NotFound();
            }

            var weeklyMenu = await _context.WeeklyMenus
                .Include(w => w.CreatedBy)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (weeklyMenu == null)
            {
                return NotFound();
            }

            return View(weeklyMenu);
        }

        // GET: WeeklyMenu/Create
        public IActionResult Create()
        {
            if (User.Claims.FirstOrDefault(c => c.Type == "isAdmin")?.Value != "true") return Forbid();
            ViewData["CreatedById"] = new SelectList(_context.AspNetUsers, "Id", "Id");
            // pass available weekly plans for admin selection
            var plans = _context.WeeklyPlans.Include(p => p.Days).ToList();
            ViewBag.WeeklyPlans = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(plans, "Id", "WeekStart");
            ViewBag.PlansJson = System.Text.Json.JsonSerializer.Serialize(plans.Select(p => new { p.Id, WeekStart = p.WeekStart, Days = p.Days.Select(d => new { d.DayOfWeek, d.BreakfastPrice, d.LunchPrice, d.DinnerPrice }) }));
            // if user is authenticated, set default created by
            var uid = User.Claims.FirstOrDefault(c => c.Type == "userId")?.Value;
            ViewData["CreatedById"] = new SelectList(_context.AspNetUsers, "Id", "Id", uid);
            return View();
        }

        // POST: WeeklyMenu/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,WeekStartDate,BreakfastRate,LunchRate,DinnerRate,CreatedById,CreatedAt")] WeeklyMenu weeklyMenu)
        {
            if (User.Claims.FirstOrDefault(c => c.Type == "isAdmin")?.Value != "true") return Forbid();
            if (ModelState.IsValid)
            {
                _context.Add(weeklyMenu);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CreatedById"] = new SelectList(_context.AspNetUsers, "Id", "Id", weeklyMenu.CreatedById);
            return View(weeklyMenu);
        }

        // GET: WeeklyMenu/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (User.Claims.FirstOrDefault(c => c.Type == "isAdmin")?.Value != "true") return Forbid();
            if (id == null)
            {
                return NotFound();
            }

            var weeklyMenu = await _context.WeeklyMenus.FindAsync(id);
            if (weeklyMenu == null)
            {
                return NotFound();
            }
            ViewData["CreatedById"] = new SelectList(_context.AspNetUsers, "Id", "Id", weeklyMenu.CreatedById);
            return View(weeklyMenu);
        }

        // POST: WeeklyMenu/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,WeekStartDate,BreakfastRate,LunchRate,DinnerRate,CreatedById,CreatedAt")] WeeklyMenu weeklyMenu)
        {
            if (User.Claims.FirstOrDefault(c => c.Type == "isAdmin")?.Value != "true") return Forbid();
            if (id != weeklyMenu.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(weeklyMenu);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!WeeklyMenuExists(weeklyMenu.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["CreatedById"] = new SelectList(_context.AspNetUsers, "Id", "Id", weeklyMenu.CreatedById);
            return View(weeklyMenu);
        }

        // GET: WeeklyMenu/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (User.Claims.FirstOrDefault(c => c.Type == "isAdmin")?.Value != "true") return Forbid();
            if (id == null)
            {
                return NotFound();
            }

            var weeklyMenu = await _context.WeeklyMenus
                .Include(w => w.CreatedBy)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (weeklyMenu == null)
            {
                return NotFound();
            }

            return View(weeklyMenu);
        }

        // POST: WeeklyMenu/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (User.Claims.FirstOrDefault(c => c.Type == "isAdmin")?.Value != "true") return Forbid();
            var weeklyMenu = await _context.WeeklyMenus.FindAsync(id);
            if (weeklyMenu != null)
            {
                _context.WeeklyMenus.Remove(weeklyMenu);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool WeeklyMenuExists(int id)
        {
            return _context.WeeklyMenus.Any(e => e.Id == id);
        }
    }
}
