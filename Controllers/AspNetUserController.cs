using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using mess_management.Models;

using Microsoft.AspNetCore.Authorization;

namespace mess_management.Controllers
{
    [Authorize]
    public class AspNetUserController : Controller
    {
        private readonly AppDbContext _context;

        public AspNetUserController(AppDbContext context)
        {
            _context = context;
        }

        // GET: AspNetUser
        public async Task<IActionResult> Index()
        {
            if (!User.HasClaim("isAdmin", "true")) return Forbid();
            return View(await _context.AspNetUsers.ToListAsync());
        }

        // GET: AspNetUser/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var aspNetUser = await _context.AspNetUsers
                .FirstOrDefaultAsync(m => m.Id == id);
            if (aspNetUser == null)
            {
                return NotFound();
            }

            return View(aspNetUser);
        }

        // GET: AspNetUser/Create
        public IActionResult Create()
        {
            if (!User.HasClaim("isAdmin", "true")) return Forbid();
            return View();
        }

        // POST: AspNetUser/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("FullName,JoinedDate,Email,IsAdmin")] AspNetUser aspNetUser, string initialPassword)
        {
            if (!User.HasClaim("isAdmin", "true")) return Forbid();

            if (string.IsNullOrWhiteSpace(initialPassword))
            {
                ModelState.AddModelError("initialPassword", "Initial password is required.");
            }

            // password rules: min 8, upper, lower, digit, special
            if (!string.IsNullOrWhiteSpace(initialPassword))
            {
                var password = initialPassword.Trim();
                if (password.Length < 8 ||
                    !password.Any(char.IsUpper) ||
                    !password.Any(char.IsLower) ||
                    !password.Any(char.IsDigit) ||
                    !password.Any(ch => !char.IsLetterOrDigit(ch)))
                {
                    ModelState.AddModelError("initialPassword", "Password must be at least 8 characters and include upper, lower, digit and special character.");
                }
            }

            ModelState.Remove("Id");
            if (ModelState.IsValid)
            {
                // check if email already exists
                if (_context.AspNetUsers.Any(u => u.Email == aspNetUser.Email))
                {
                    ModelState.AddModelError("Email", "Email is already in use.");
                    return View(aspNetUser);
                }

                // generate unique id
                aspNetUser.Id = Guid.NewGuid().ToString();
                
                // hash password
                aspNetUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(initialPassword);
                aspNetUser.IsPasswordChanged = false;
                if (!aspNetUser.JoinedDate.HasValue) aspNetUser.JoinedDate = DateTime.Now;
                _context.Add(aspNetUser);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "User created successfully.";
                return RedirectToAction(nameof(Index));
            }
            return View(aspNetUser);
        }

        // GET: AspNetUser/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (!User.HasClaim("isAdmin", "true")) return Forbid();
            if (id == null)
            {
                return NotFound();
            }

            var aspNetUser = await _context.AspNetUsers.FindAsync(id);
            if (aspNetUser == null)
            {
                return NotFound();
            }
            return View(aspNetUser);
        }

        // POST: AspNetUser/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Id,FullName,JoinedDate,IsPasswordChanged,Email,IsAdmin")] AspNetUser aspNetUser)
        {
            if (id != aspNetUser.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // do not overwrite password hash here
                    var existing = await _context.AspNetUsers.FindAsync(id);
                    if (existing == null) return NotFound();
                    existing.FullName = aspNetUser.FullName;
                    existing.JoinedDate = aspNetUser.JoinedDate;
                    existing.IsPasswordChanged = aspNetUser.IsPasswordChanged;
                    existing.Email = aspNetUser.Email;
                    existing.IsAdmin = aspNetUser.IsAdmin;

                    _context.Update(existing);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AspNetUserExists(aspNetUser.Id))
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
            return View(aspNetUser);
        }

        // GET: AspNetUser/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (!User.HasClaim("isAdmin", "true")) return Forbid();
            if (id == null)
            {
                return NotFound();
            }

            var aspNetUser = await _context.AspNetUsers
                .FirstOrDefaultAsync(m => m.Id == id);
            if (aspNetUser == null)
            {
                return NotFound();
            }

            return View(aspNetUser);
        }

        // POST: AspNetUser/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (!User.HasClaim("isAdmin", "true")) return Forbid();

            var aspNetUser = await _context.AspNetUsers.FindAsync(id);
            if (aspNetUser != null)
            {
                // 1. Delete Attendance Records
                var attendances = _context.TeacherAttendances.Where(a => a.TeacherId == id);
                _context.TeacherAttendances.RemoveRange(attendances);

                // 2. Delete Monthly Bills
                var monthlyBills = _context.MonthlyBills.Where(b => b.TeacherId == id);
                _context.MonthlyBills.RemoveRange(monthlyBills);
                
                // 3. Delete Bills
                var bills = _context.Bills.Where(b => b.TeacherId == id);
                _context.Bills.RemoveRange(bills);

                // 4. Handle Weekly Menus (Set CreatedById to null)
                var weeklyMenus = _context.WeeklyMenus.Where(m => m.CreatedById == id);
                foreach (var menu in weeklyMenus)
                {
                    menu.CreatedById = null;
                }
                
                // 5. Delete the User
                _context.AspNetUsers.Remove(aspNetUser);
                
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool AspNetUserExists(string id)
        {
            return _context.AspNetUsers.Any(e => e.Id == id);
        }
    }
}
