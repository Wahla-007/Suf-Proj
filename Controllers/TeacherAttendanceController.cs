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
    public class TeacherAttendanceController : Controller
    {
        private readonly AppDbContext _context;

        public TeacherAttendanceController(AppDbContext context)
        {
            _context = context;
        }

        // GET: TeacherAttendance
        public async Task<IActionResult> Index(string teacherSearch, DateOnly? startDate, DateOnly? endDate, bool? status)
        {
            if (User.Claims.FirstOrDefault(c => c.Type == "isAdmin")?.Value != "true") return Forbid();
            var query = _context.TeacherAttendances
                .Include(t => t.Teacher)
                .AsQueryable();

            if (!string.IsNullOrEmpty(teacherSearch))
            {
                query = query.Where(t => t.Teacher.FullName.Contains(teacherSearch) || t.Teacher.Email.Contains(teacherSearch));
            }

            if (startDate.HasValue)
            {
                query = query.Where(t => t.Date >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(t => t.Date <= endDate.Value);
            }

            if (status.HasValue)
            {
                query = query.Where(t => t.IsVerified == status.Value);
            }

            // Keep track of filter values for the UI
            ViewData["TeacherSearch"] = teacherSearch;
            ViewData["StartDate"] = startDate?.ToString("yyyy-MM-dd");
            ViewData["EndDate"] = endDate?.ToString("yyyy-MM-dd");
            ViewData["Status"] = status;

            return View(await query.OrderByDescending(t => t.Date).ToListAsync());
        }

        // GET: TeacherAttendance/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (User.Claims.FirstOrDefault(c => c.Type == "isAdmin")?.Value != "true") return Forbid();
            if (id == null)
            {
                return NotFound();
            }

            var teacherAttendance = await _context.TeacherAttendances
                .Include(t => t.Teacher)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (teacherAttendance == null)
            {
                return NotFound();
            }

            return View(teacherAttendance);
        }

        // GET: TeacherAttendance/Create
        public IActionResult Create()
        {
            if (User.Claims.FirstOrDefault(c => c.Type == "isAdmin")?.Value != "true") return Forbid();
            ViewData["TeacherId"] = new SelectList(_context.AspNetUsers, "Id", "FullName");
            return View();
        }

        // POST: TeacherAttendance/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,TeacherId,Date,Breakfast,Lunch,Dinner,MarkedBy,IsVerified,VerificationNote,VerifiedAt")] TeacherAttendance teacherAttendance)
        {
            if (User.Claims.FirstOrDefault(c => c.Type == "isAdmin")?.Value != "true") return Forbid();
            if (ModelState.IsValid)
            {
                // check for duplicates
                var exists = await _context.TeacherAttendances.AnyAsync(a => a.TeacherId == teacherAttendance.TeacherId && a.Date == teacherAttendance.Date);
                if (exists)
                {
                    ModelState.AddModelError("", "Attendance for this teacher on this date already exists.");
                }
                else
                {
                    _context.Add(teacherAttendance);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
            ViewData["TeacherId"] = new SelectList(_context.AspNetUsers, "Id", "FullName", teacherAttendance.TeacherId);
            return View(teacherAttendance);
        }

        // GET: TeacherAttendance/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (User.Claims.FirstOrDefault(c => c.Type == "isAdmin")?.Value != "true") return Forbid();
            if (id == null)
            {
                return NotFound();
            }

            var teacherAttendance = await _context.TeacherAttendances.FindAsync(id);
            if (teacherAttendance == null)
            {
                return NotFound();
            }
            ViewData["TeacherId"] = new SelectList(_context.AspNetUsers, "Id", "FullName", teacherAttendance.TeacherId);
            return View(teacherAttendance);
        }

        // POST: TeacherAttendance/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,TeacherId,Date,Breakfast,Lunch,Dinner,MarkedBy,IsVerified,VerificationNote,VerifiedAt")] TeacherAttendance teacherAttendance)
        {
            if (User.Claims.FirstOrDefault(c => c.Type == "isAdmin")?.Value != "true") return Forbid();
            if (id != teacherAttendance.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(teacherAttendance);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TeacherAttendanceExists(teacherAttendance.Id))
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
            ViewData["TeacherId"] = new SelectList(_context.AspNetUsers, "Id", "FullName", teacherAttendance.TeacherId);
            return View(teacherAttendance);
        }

        // GET: TeacherAttendance/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (User.Claims.FirstOrDefault(c => c.Type == "isAdmin")?.Value != "true") return Forbid();
            if (id == null)
            {
                return NotFound();
            }

            var teacherAttendance = await _context.TeacherAttendances
                .Include(t => t.Teacher)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (teacherAttendance == null)
            {
                return NotFound();
            }

            return View(teacherAttendance);
        }

        // POST: TeacherAttendance/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (User.Claims.FirstOrDefault(c => c.Type == "isAdmin")?.Value != "true") return Forbid();
            var teacherAttendance = await _context.TeacherAttendances.FindAsync(id);
            if (teacherAttendance != null)
            {
                _context.TeacherAttendances.Remove(teacherAttendance);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: TeacherAttendance/Disputes
        public async Task<IActionResult> Disputes()
        {
            if (User.Claims.FirstOrDefault(c => c.Type == "isAdmin")?.Value != "true") return Forbid();

            var disputes = await _context.TeacherAttendances
                .Include(t => t.Teacher)
                .Where(t => t.DisputeStatus == "Raised")
                .OrderByDescending(t => t.Date)
                .ToListAsync();

            return View(disputes);
        }

        // POST: TeacherAttendance/ApproveDispute/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveDispute(int id, string returnUrl = null)
        {
            if (User.Claims.FirstOrDefault(c => c.Type == "isAdmin")?.Value != "true") return Forbid();
            var record = await _context.TeacherAttendances.FindAsync(id);
            if (record == null) return NotFound();

            record.DisputeStatus = "Approved";
            record.IsVerified = false; // Mark as invalid/cancelled
            record.Breakfast = false;
            record.Lunch = false;
            record.Dinner = false;
            record.VerificationNote = "Dispute Approved: Record cancelled.";
            record.VerifiedAt = DateTime.Now;

            _context.Update(record);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Dispute approved. Record has been cancelled.";
            
            if (!string.IsNullOrEmpty(returnUrl)) return LocalRedirect(returnUrl);
            return RedirectToAction(nameof(Disputes));
        }

        // POST: TeacherAttendance/RejectDispute/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectDispute(int id, string adminNote, string returnUrl = null)
        {
            if (User.Claims.FirstOrDefault(c => c.Type == "isAdmin")?.Value != "true") return Forbid();
            var record = await _context.TeacherAttendances.FindAsync(id);
            if (record == null) return NotFound();

            record.DisputeStatus = "Rejected";
            record.IsVerified = true; // Keep as valid/charged
            record.VerificationNote = $"Dispute Rejected: {adminNote}";
            record.VerifiedAt = DateTime.Now;

            _context.Update(record);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Dispute rejected. Record remains active.";
            
            if (!string.IsNullOrEmpty(returnUrl)) return LocalRedirect(returnUrl);
            return RedirectToAction(nameof(Disputes));
        }

        private bool TeacherAttendanceExists(int id)
        {
            return _context.TeacherAttendances.Any(e => e.Id == id);
        }
    }
}
