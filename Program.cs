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

            // Add services to the container.
            Console.WriteLine("📦 Adding services...");
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(
                    "Server=(localdb)\\MSSQLLocalDB;Database=mess;Trusted_Connection=True;TrustServerCertificate=True;Connection Timeout=60;",
                    sqlServerOptions => sqlServerOptions.EnableRetryOnFailure()));

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
    }
}
