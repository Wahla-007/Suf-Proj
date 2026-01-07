using System;
using System.Collections.Generic;
using System.Linq;
using mess_management.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using mess_management.Services;

namespace mess_management
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("🚀 Application starting...");
            
            var builder = WebApplication.CreateBuilder(args);

            // Ensure Data directory exists for SQLite database
            var dataDir = Path.Combine(builder.Environment.ContentRootPath, "Data");
            Directory.CreateDirectory(dataDir);
            var dbPath = Path.Combine(dataDir, "mess.db");
            Console.WriteLine($"📂 Database path: {dbPath}");

            // Add services to the container.
            Console.WriteLine("📦 Adding services...");
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlite($"Data Source={dbPath}"));

            builder.Services.AddControllersWithViews()
                .AddRazorRuntimeCompilation();

            // Configure Antiforgery to look for the header
            builder.Services.AddAntiforgery(options => options.HeaderName = "RequestVerificationToken");

            // Hybrid Authentication (Cookies for UI, JWT for API)
            var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();
            builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
            builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = "CookieAuth";
                options.DefaultSignInScheme = "CookieAuth";
                options.DefaultChallengeScheme = "CookieAuth";
            })
            .AddCookie("CookieAuth", options =>
            {
                options.LoginPath = "/Account/Login";
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.SameAsRequest;
                options.Cookie.SameSite = SameSiteMode.Lax;
                options.ExpireTimeSpan = TimeSpan.FromHours(8);
            })
            .AddJwtBearer(Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings?.Issuer,
                    ValidAudience = jwtSettings?.Audience,
                    IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
                        System.Text.Encoding.UTF8.GetBytes(jwtSettings?.SecretKey ?? ""))
                };
            });

            var app = builder.Build();

            // Initialize database with migrations and seed data
            Console.WriteLine("🗄️ Initializing database...");
            using (var scope = app.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                try
                {
                    // Ensure database is created and migrations applied
                    dbContext.Database.EnsureCreated();
                    Console.WriteLine("✅ Database created/verified successfully!");
                    
                    // Seed data if no users exist
                    if (!dbContext.AspNetUsers.Any())
                    {
                        Console.WriteLine("🌱 Seeding database with initial data...");
                        SeedDatabase(dbContext);
                        Console.WriteLine("✅ Database seeded successfully!");
                    }
                    else
                    {
                        Console.WriteLine("📊 Database already has data, skipping seed.");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Database initialization error: {ex.Message}");
                    Console.WriteLine($"Stack trace: {ex.StackTrace}");
                    // Don't throw - allow app to start, error will be visible in logs
                }
            }

            Console.WriteLine("🎯 Application configured!");

            // Middleware pipeline
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            // app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            Console.WriteLine("🌐 Starting server...");
            app.Run();
        }

        /// <summary>
        /// Seeds the database with Pakistani users, dishes, and sample data
        /// </summary>
        private static void SeedDatabase(AppDbContext dbContext)
        {
            // BCrypt hash for password "Password123!" - pre-generated for performance
            var passwordHash = BCrypt.Net.BCrypt.HashPassword("Password123!");
            var adminPasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123");

            // Seed Admin User
            var admin = new AspNetUser
            {
                Id = Guid.NewGuid().ToString(),
                Email = "admin@mess.pk",
                FullName = "Muhammad Arslan (Admin)",
                PasswordHash = adminPasswordHash,
                IsAdmin = true,
                IsPasswordChanged = true,
                JoinedDate = DateTime.UtcNow.AddMonths(-6)
            };
            dbContext.AspNetUsers.Add(admin);

            // Seed Teachers with Pakistani Names (as requested)
            var teachers = new List<AspNetUser>
            {
                new AspNetUser
                {
                    Id = Guid.NewGuid().ToString(),
                    Email = "hamna.nasir@mess.pk",
                    FullName = "Hamna Nasir",
                    PasswordHash = passwordHash,
                    IsAdmin = false,
                    IsPasswordChanged = true,
                    JoinedDate = DateTime.UtcNow.AddMonths(-5)
                },
                new AspNetUser
                {
                    Id = Guid.NewGuid().ToString(),
                    Email = "sufwan.masood@mess.pk",
                    FullName = "Sufwan Masood",
                    PasswordHash = passwordHash,
                    IsAdmin = false,
                    IsPasswordChanged = true,
                    JoinedDate = DateTime.UtcNow.AddMonths(-4)
                },
                new AspNetUser
                {
                    Id = Guid.NewGuid().ToString(),
                    Email = "tayyab.nasir@mess.pk",
                    FullName = "Tayyab Nasir",
                    PasswordHash = passwordHash,
                    IsAdmin = false,
                    IsPasswordChanged = false,
                    JoinedDate = DateTime.UtcNow.AddMonths(-3)
                },
                new AspNetUser
                {
                    Id = Guid.NewGuid().ToString(),
                    Email = "abdul.hanan@mess.pk",
                    FullName = "Abdul Hanan",
                    PasswordHash = passwordHash,
                    IsAdmin = false,
                    IsPasswordChanged = true,
                    JoinedDate = DateTime.UtcNow.AddMonths(-5)
                },
                new AspNetUser
                {
                    Id = Guid.NewGuid().ToString(),
                    Email = "fatima.khan@mess.pk",
                    FullName = "Fatima Khan",
                    PasswordHash = passwordHash,
                    IsAdmin = false,
                    IsPasswordChanged = false,
                    JoinedDate = DateTime.UtcNow.AddMonths(-2)
                },
                new AspNetUser
                {
                    Id = Guid.NewGuid().ToString(),
                    Email = "ali.raza@mess.pk",
                    FullName = "Ali Raza",
                    PasswordHash = passwordHash,
                    IsAdmin = false,
                    IsPasswordChanged = true,
                    JoinedDate = DateTime.UtcNow.AddMonths(-4)
                },
                new AspNetUser
                {
                    Id = Guid.NewGuid().ToString(),
                    Email = "ayesha.malik@mess.pk",
                    FullName = "Ayesha Malik",
                    PasswordHash = passwordHash,
                    IsAdmin = false,
                    IsPasswordChanged = true,
                    JoinedDate = DateTime.UtcNow.AddMonths(-3)
                },
                new AspNetUser
                {
                    Id = Guid.NewGuid().ToString(),
                    Email = "usman.ahmed@mess.pk",
                    FullName = "Usman Ahmed",
                    PasswordHash = passwordHash,
                    IsAdmin = false,
                    IsPasswordChanged = false,
                    JoinedDate = DateTime.UtcNow.AddMonths(-1)
                },
                new AspNetUser
                {
                    Id = Guid.NewGuid().ToString(),
                    Email = "zainab.ali@mess.pk",
                    FullName = "Zainab Ali",
                    PasswordHash = passwordHash,
                    IsAdmin = false,
                    IsPasswordChanged = true,
                    JoinedDate = DateTime.UtcNow.AddMonths(-2)
                },
                new AspNetUser
                {
                    Id = Guid.NewGuid().ToString(),
                    Email = "bilal.hassan@mess.pk",
                    FullName = "Bilal Hassan",
                    PasswordHash = passwordHash,
                    IsAdmin = false,
                    IsPasswordChanged = true,
                    JoinedDate = DateTime.UtcNow.AddMonths(-5)
                }
            };
            dbContext.AspNetUsers.AddRange(teachers);

            // Seed Default Settings
            var settings = new List<Setting>
            {
                new Setting { Key = "WaterFee", Value = "500" },
                new Setting { Key = "DefaultBreakfastRate", Value = "120" },
                new Setting { Key = "DefaultLunchRate", Value = "200" },
                new Setting { Key = "DefaultDinnerRate", Value = "180" },
                new Setting { Key = "MessName", Value = "COMSATS Mess" },
                new Setting { Key = "Currency", Value = "PKR" }
            };
            dbContext.Settings.AddRange(settings);

            // Pakistani Breakfast Options
            var breakfasts = new[] {
                "Aloo Paratha", "Halwa Puri", "Channay", "Nihari", "Paye",
                "Omelette & Toast", "Daal Paratha", "Anda Paratha", "Siri Paye", "Nashta Thali"
            };
            
            // Pakistani Lunch Options  
            var lunches = new[] {
                "Chicken Biryani", "Mutton Pulao", "Beef Korma", "Daal Chawal", "Chicken Karahi",
                "Aloo Gosht", "Qeema Rice", "Chapli Kebab", "Chicken Jalfrezi", "Mixed Sabzi"
            };
            
            // Pakistani Dinner Options
            var dinners = new[] {
                "Chicken Tikka", "Seekh Kebab", "Mutton Handi", "Butter Chicken", "Palak Paneer",
                "Chicken Korma", "Beef Nihari", "Fish Fry", "Malai Boti", "BBQ Platter"
            };

            // Seed Multiple Weekly Plans (last 4 weeks)
            var weeklyPlans = new List<WeeklyPlan>();
            for (int week = 0; week < 4; week++)
            {
                var weekStart = DateTime.UtcNow.Date.AddDays(-7 * week);
                // Adjust to Monday
                while (weekStart.DayOfWeek != DayOfWeek.Monday) weekStart = weekStart.AddDays(-1);

                var plan = new WeeklyPlan
                {
                    WeekStart = weekStart,
                    CreatedAt = weekStart,
                    CreatedById = admin.Id,
                    Days = new List<WeeklyPlanDay>()
                };

                var rnd = new Random(week * 100);
                for (int d = 0; d < 7; d++)
                {
                    plan.Days.Add(new WeeklyPlanDay
                    {
                        DayOfWeek = d == 6 ? 0 : d + 1, // Monday=1, Sunday=0
                        BreakfastName = breakfasts[rnd.Next(breakfasts.Length)],
                        LunchName = lunches[rnd.Next(lunches.Length)],
                        DinnerName = dinners[rnd.Next(dinners.Length)],
                        BreakfastPrice = 80 + rnd.Next(60),   // 80-140 PKR
                        LunchPrice = 150 + rnd.Next(100),      // 150-250 PKR
                        DinnerPrice = 120 + rnd.Next(130)      // 120-250 PKR
                    });
                }
                weeklyPlans.Add(plan);
            }
            dbContext.WeeklyPlans.AddRange(weeklyPlans);

            // Seed Sample Attendance for the current month (25 days for each teacher)
            var currentMonth = DateTime.UtcNow;
            var random = new Random(42); // Fixed seed for reproducible results
            var attendanceList = new List<TeacherAttendance>();
            
            foreach (var teacher in teachers)
            {
                int daysInMonth = DateTime.DaysInMonth(currentMonth.Year, currentMonth.Month);
                int daysToSeed = Math.Min(25, daysInMonth);
                
                for (int day = 1; day <= daysToSeed; day++)
                {
                    var date = new DateTime(currentMonth.Year, currentMonth.Month, day);
                    
                    // Weekday pattern - more likely to attend
                    bool isWeekday = date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday;
                    int breakfastChance = isWeekday ? 75 : 40;
                    int lunchChance = isWeekday ? 90 : 50;
                    int dinnerChance = isWeekday ? 70 : 80;
                    
                    var attendance = new TeacherAttendance
                    {
                        TeacherId = teacher.Id,
                        Date = DateOnly.FromDateTime(date),
                        Breakfast = random.Next(100) < breakfastChance,
                        Lunch = random.Next(100) < lunchChance,
                        Dinner = random.Next(100) < dinnerChance,
                        MarkedBy = admin.Email,
                        IsVerified = day <= 20, // First 20 days verified
                        VerifiedAt = day <= 20 ? DateTime.UtcNow.AddDays(-5) : null,
                        DisputeStatus = "None"
                    };
                    attendanceList.Add(attendance);
                }
            }
            dbContext.TeacherAttendances.AddRange(attendanceList);

            // Seed Weekly Menus with rates
            var weeklyMenus = new List<WeeklyMenu>();
            var menuRnd = new Random(99);
            for (int week = 0; week < 4; week++)
            {
                var weekStart = DateTime.UtcNow.Date.AddDays(-7 * week);
                while (weekStart.DayOfWeek != DayOfWeek.Monday) weekStart = weekStart.AddDays(-1);
                
                var menu = new WeeklyMenu
                {
                    WeekStartDate = weekStart,
                    CreatedAt = weekStart,
                    CreatedById = admin.Id,
                    BreakfastRate = 80 + menuRnd.Next(60),   // 80-140 PKR
                    LunchRate = 150 + menuRnd.Next(100),      // 150-250 PKR
                    DinnerRate = 120 + menuRnd.Next(130)      // 120-250 PKR
                };
                weeklyMenus.Add(menu);
            }
            dbContext.WeeklyMenus.AddRange(weeklyMenus);

            dbContext.SaveChanges();
            Console.WriteLine($"✅ Seeded: 1 admin, {teachers.Count} teachers, {settings.Count} settings, {weeklyPlans.Count} weekly plans, {weeklyMenus.Count} menus, {attendanceList.Count} attendance records");
        }
    }
}
