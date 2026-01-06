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
    public class MonthlyBillController : Controller
    {
        private readonly AppDbContext _context;

        public MonthlyBillController(AppDbContext context)
        {
            _context = context;
        }

        // GET: MonthlyBill
        public async Task<IActionResult> Index()
        {
            if (User.Claims.FirstOrDefault(c => c.Type == "isAdmin")?.Value != "true") return Forbid();
            var appDbContext = _context.MonthlyBills.Include(m => m.Teacher);
            return View(await appDbContext.ToListAsync());
        }

        // GET: MonthlyBill/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (User.Claims.FirstOrDefault(c => c.Type == "isAdmin")?.Value != "true") return Forbid();
            if (id == null)
            {
                return NotFound();
            }

            var monthlyBill = await _context.MonthlyBills
                .Include(m => m.Teacher)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (monthlyBill == null)
            {
                return NotFound();
            }

            return View(monthlyBill);
        }

        // GET: MonthlyBill/Create
        public IActionResult Create()
        {
            if (User.Claims.FirstOrDefault(c => c.Type == "isAdmin")?.Value != "true") return Forbid();
            ViewData["TeacherId"] = new SelectList(_context.AspNetUsers, "Id", "Id");
            return View();
        }

        // POST: MonthlyBill/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,TeacherId,Year,Month,TotalMeals,FoodAmount,WaterShare,PreviousDue,TotalDue,PaidAmount,Status,GeneratedOn,PaidOn,PaymentToken")] MonthlyBill monthlyBill)
        {
            if (User.Claims.FirstOrDefault(c => c.Type == "isAdmin")?.Value != "true") return Forbid();
            if (ModelState.IsValid)
            {
                _context.Add(monthlyBill);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["TeacherId"] = new SelectList(_context.AspNetUsers, "Id", "Id", monthlyBill.TeacherId);
            return View(monthlyBill);
        }

        // GET: MonthlyBill/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (User.Claims.FirstOrDefault(c => c.Type == "isAdmin")?.Value != "true") return Forbid();
            if (id == null)
            {
                return NotFound();
            }

            var monthlyBill = await _context.MonthlyBills.FindAsync(id);
            if (monthlyBill == null)
            {
                return NotFound();
            }
            ViewData["TeacherId"] = new SelectList(_context.AspNetUsers, "Id", "Id", monthlyBill.TeacherId);
            return View(monthlyBill);
        }

        // POST: MonthlyBill/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,TeacherId,Year,Month,TotalMeals,FoodAmount,WaterShare,PreviousDue,TotalDue,PaidAmount,Status,GeneratedOn,PaidOn,PaymentToken")] MonthlyBill monthlyBill)
        {
            if (User.Claims.FirstOrDefault(c => c.Type == "isAdmin")?.Value != "true") return Forbid();
            if (id != monthlyBill.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(monthlyBill);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MonthlyBillExists(monthlyBill.Id))
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
            ViewData["TeacherId"] = new SelectList(_context.AspNetUsers, "Id", "Id", monthlyBill.TeacherId);
            return View(monthlyBill);
        }

        // GET: MonthlyBill/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (User.Claims.FirstOrDefault(c => c.Type == "isAdmin")?.Value != "true") return Forbid();
            if (id == null)
            {
                return NotFound();
            }

            var monthlyBill = await _context.MonthlyBills
                .Include(m => m.Teacher)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (monthlyBill == null)
            {
                return NotFound();
            }

            return View(monthlyBill);
        }

        // POST: MonthlyBill/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (User.Claims.FirstOrDefault(c => c.Type == "isAdmin")?.Value != "true") return Forbid();
            var monthlyBill = await _context.MonthlyBills.FindAsync(id);
            if (monthlyBill != null)
            {
                _context.MonthlyBills.Remove(monthlyBill);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MonthlyBillExists(int id)
        {
            return _context.MonthlyBills.Any(e => e.Id == id);
        }
    }
}
