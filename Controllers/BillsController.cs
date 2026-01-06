using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using mess_management.Models;

namespace mess_management.Controllers
{
    [Authorize]
    public class BillsController : Controller
    {
        private readonly AppDbContext _context;

        public BillsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Bills
        public async Task<IActionResult> Index()
        {
            if (User.Claims.FirstOrDefault(c => c.Type == "isAdmin")?.Value != "true") return Forbid();
            var appDbContext = _context.Bills.Include(b => b.Teacher).Include(b => b.Lines);
            return View(await appDbContext.ToListAsync());
        }

        // GET: Bills/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (User.Claims.FirstOrDefault(c => c.Type == "isAdmin")?.Value != "true") return Forbid();
            if (id == null) return NotFound();
            var bill = await _context.Bills.Include(b => b.Lines).Include(b => b.Payments).Include(b => b.Teacher).FirstOrDefaultAsync(b => b.Id == id);
            if (bill == null) return NotFound();
            return View(bill);
        }

        // POST: Bills/MarkPaid/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkPaid(int id)
        {
            if (User.Claims.FirstOrDefault(c => c.Type == "isAdmin")?.Value != "true") return Forbid();
            var bill = await _context.Bills.FindAsync(id);
            if (bill == null) return NotFound();
            bill.Status = "Paid";
            bill.PaidAmount = bill.TotalDue;
            bill.PaidOn = DateTime.Now;
            _context.Update(bill);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Bill marked as paid successfully!";
            return RedirectToAction(nameof(Index));
        }
    }
}