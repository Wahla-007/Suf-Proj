using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using mess_management.Models;

namespace mess_management.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ILogger<AccountController> _logger;

        public AccountController(AppDbContext context, ILogger<AccountController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true) return RedirectToAction("Index", "Home");
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (!ModelState.IsValid) return View(model);

            var user = _context.AspNetUsers.FirstOrDefault(u => u.Email == model.Email);
            if (user == null)
            {
                _logger.LogWarning("User not found: {Email}", model.Email);
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return View(model);
            }

            bool passwordMatches = false;
            if (!string.IsNullOrEmpty(user.PasswordHash))
            {
                try
                {
                    if (user.PasswordHash.StartsWith("$2"))
                    {
                        _logger.LogInformation("Attempting BCrypt verification for {Email}", user.Email);
                        passwordMatches = BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "BCrypt verification failed for {Email}", user.Email);
                    // Ignore bcrypt errors/exceptions and try plain text
                }

                if (!passwordMatches)
                {
                    // Fallback to plain text comparison (trim both for safety)
                    var storedHash = user.PasswordHash?.Trim() ?? "";
                    var inputPassword = model.Password?.Trim() ?? "";
                    passwordMatches = (storedHash == inputPassword);
                    _logger.LogInformation("Plain text comparison for {Email}: Stored='{Stored}', Input='{Input}', Match={Match}", 
                        user.Email, storedHash, inputPassword, passwordMatches);
                }
            }

            if (!passwordMatches)
            {
                _logger.LogWarning("Password mismatch for {Email}", user.Email);
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return View(model);
            }

            // sign in
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Email ?? user.Id),
                new Claim("userId", user.Id),
                new Claim("isAdmin", user.IsAdmin == true ? "true" : "false")
            };

            var identity = new ClaimsIdentity(claims, "CookieAuth");
            var principal = new ClaimsPrincipal(identity);
            await HttpContext.SignInAsync("CookieAuth", principal);

            if ((user.IsPasswordChanged == false || user.IsPasswordChanged == null) && (user.IsAdmin != true))
            {
                return RedirectToAction("ChangePassword", new { firstTime = true });
            }

            // Redirect based on user role - Admin goes to Admin Dashboard, Teachers go to Teacher Dashboard
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl) && !returnUrl.Contains("/Home"))
            {
                return Redirect(returnUrl);
            }
            
            if (user.IsAdmin == true)
            {
                _logger.LogInformation("Admin {Email} logged in, redirecting to Admin Dashboard", user.Email);
                return RedirectToAction("Index", "Admin");
            }
            else
            {
                _logger.LogInformation("Teacher {Email} logged in, redirecting to Teacher Dashboard", user.Email);
                return RedirectToAction("Dashboard", "Teacher");
            }
        }

        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("CookieAuth");
            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        [HttpGet]
        public IActionResult ChangePassword(bool firstTime = false)
        {
            ViewData["FirstTime"] = firstTime;
            return View();
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var userId = User.Claims.FirstOrDefault(c => c.Type == "userId")?.Value;
            if (userId == null) return Forbid();

            var user = await _context.AspNetUsers.FindAsync(userId);
            if (user == null) return NotFound();

            bool currentMatches = false;
            if (!string.IsNullOrEmpty(user.PasswordHash))
            {
                var inputCurrent = model.CurrentPassword ?? "";
                var inputCurrentTrimmed = inputCurrent.Trim();
                
                // Try BCrypt verification - try trimmed first as it's the standard for this app
                if (user.PasswordHash.StartsWith("$2"))
                {
                    try
                    {
                        currentMatches = BCrypt.Net.BCrypt.Verify(inputCurrentTrimmed, user.PasswordHash);
                        // Fallback to raw if trimmed fails (in case user explicitly used spaces)
                        if (!currentMatches && inputCurrent != inputCurrentTrimmed)
                        {
                            currentMatches = BCrypt.Net.BCrypt.Verify(inputCurrent, user.PasswordHash);
                        }
                    }
                    catch { currentMatches = false; }
                }
                
                // Fallback for PlainText (Legacy or manual DB entry)
                if (!currentMatches)
                {
                    currentMatches = (inputCurrentTrimmed == user.PasswordHash || inputCurrent == user.PasswordHash);
                }
            }

            if (!currentMatches)
            {
                return View(model);
            }

            // validate new password complexity
            var newPass = model.NewPassword?.Trim() ?? "";
            if (newPass.Length < 8)
            {
                ModelState.AddModelError(string.Empty, "New password must be at least 8 characters long.");
                return View(model);
            }

            // Hash the trimmed password for consistency
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPass);
            user.IsPasswordChanged = true;
            _context.Update(user);
            await _context.SaveChangesAsync();

            // re-issue cookie with updated claims (if needed)
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Email ?? user.Id),
                new Claim("userId", user.Id),
                new Claim("isAdmin", user.IsAdmin == true ? "true" : "false")
            };
            await HttpContext.SignInAsync("CookieAuth", new ClaimsPrincipal(new ClaimsIdentity(claims, "CookieAuth")));

            TempData["SuccessMessage"] = "Password changed successfully!";
            
            if (user.IsAdmin == true)
            {
                return RedirectToAction("Index", "Admin");
            }
            return RedirectToAction("Dashboard", "Teacher");
        }
    }
}