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
        /// Seeds the database with initial Pakistani users and settings
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
                FullName = "Admin User",
                PasswordHash = adminPasswordHash,
                IsAdmin = true,
                IsPasswordChanged = true,
                JoinedDate = DateTime.UtcNow
            };
            dbContext.AspNetUsers.Add(admin);

            // Seed Teachers with Pakistani Names
            var teachers = new List<AspNetUser>
            {
                new AspNetUser
                {
                    Id = Guid.NewGuid().ToString(),
                    Email = "hamna@mess.pk",
                    FullName = "Hamna Ahmed",
                    PasswordHash = passwordHash,
                    IsAdmin = false,
                    IsPasswordChanged = false,
                    JoinedDate = DateTime.UtcNow
                },
                new AspNetUser
                {
                    Id = Guid.NewGuid().ToString(),
                    Email = "hanan@mess.pk",
                    FullName = "Hanan Khan",
                    PasswordHash = passwordHash,
                    IsAdmin = false,
                    IsPasswordChanged = false,
                    JoinedDate = DateTime.UtcNow
                },
                new AspNetUser
                {
                    Id = Guid.NewGuid().ToString(),
                    Email = "tayyab@mess.pk",
                    FullName = "Tayyab Ali",
                    PasswordHash = passwordHash,
                    IsAdmin = false,
                    IsPasswordChanged = false,
                    JoinedDate = DateTime.UtcNow
                },
                new AspNetUser
                {
                    Id = Guid.NewGuid().ToString(),
                    Email = "danish@mess.pk",
                    FullName = "Danish Malik",
                    PasswordHash = passwordHash,
                    IsAdmin = false,
                    IsPasswordChanged = false,
                    JoinedDate = DateTime.UtcNow
                },
                new AspNetUser
                {
                    Id = Guid.NewGuid().ToString(),
                    Email = "sufwan@mess.pk",
                    FullName = "Sufwan Wahla",
                    PasswordHash = passwordHash,
                    IsAdmin = false,
                    IsPasswordChanged = false,
                    JoinedDate = DateTime.UtcNow
                }
            };
            dbContext.AspNetUsers.AddRange(teachers);

            // Seed Default Settings
            var settings = new List<Setting>
            {
                new Setting { Key = "WaterFee", Value = "500" },
                new Setting { Key = "DefaultBreakfastRate", Value = "100" },
                new Setting { Key = "DefaultLunchRate", Value = "200" },
                new Setting { Key = "DefaultDinnerRate", Value = "150" }
            };
            dbContext.Settings.AddRange(settings);

            // Seed a Weekly Plan
            var weeklyPlan = new WeeklyPlan
            {
                WeekStart = DateTime.UtcNow.Date.AddDays(-(int)DateTime.UtcNow.DayOfWeek + 1), // Start of current week (Monday)
                CreatedAt = DateTime.UtcNow,
                Days = new List<WeeklyPlanDay>
                {
                    new WeeklyPlanDay { DayOfWeek = 1, BreakfastName = "Paratha & Omelette", LunchName = "Biryani", DinnerName = "Chicken Karahi", BreakfastPrice = 100, LunchPrice = 200, DinnerPrice = 150 },
                    new WeeklyPlanDay { DayOfWeek = 2, BreakfastName = "Halwa Puri", LunchName = "Daal Chawal", DinnerName = "Mutton Pulao", BreakfastPrice = 120, LunchPrice = 150, DinnerPrice = 250 },
                    new WeeklyPlanDay { DayOfWeek = 3, BreakfastName = "Nashta Platter", LunchName = "Chicken Qorma", DinnerName = "Seekh Kebab", BreakfastPrice = 100, LunchPrice = 180, DinnerPrice = 200 },
                    new WeeklyPlanDay { DayOfWeek = 4, BreakfastName = "Aloo Paratha", LunchName = "Chapli Kebab", DinnerName = "Nihari", BreakfastPrice = 90, LunchPrice = 220, DinnerPrice = 280 },
                    new WeeklyPlanDay { DayOfWeek = 5, BreakfastName = "Chana Puri", LunchName = "Beef Biryani", DinnerName = "Palak Paneer", BreakfastPrice = 100, LunchPrice = 200, DinnerPrice = 160 },
                    new WeeklyPlanDay { DayOfWeek = 6, BreakfastName = "French Toast", LunchName = "Karahi Gosht", DinnerName = "Grilled Fish", BreakfastPrice = 80, LunchPrice = 250, DinnerPrice = 300 },
                    new WeeklyPlanDay { DayOfWeek = 0, BreakfastName = "Special Brunch", LunchName = "Chicken Handi", DinnerName = "BBQ Platter", BreakfastPrice = 150, LunchPrice = 200, DinnerPrice = 350 }
                }
            };
            dbContext.WeeklyPlans.Add(weeklyPlan);

            dbContext.SaveChanges();
            Console.WriteLine($"✅ Seeded: 1 admin, {teachers.Count} teachers, {settings.Count} settings, 1 weekly plan");
        }
    }
}
