using Microsoft.AspNetCore.Mvc;
using mess_management.Models;
using Microsoft.EntityFrameworkCore;

namespace mess_management.Controllers
{
    /// <summary>
    /// Health check controller for container orchestration and monitoring
    /// </summary>
    [Route("[controller]")]
    [ApiController]
    public class HealthController : ControllerBase
    {
        private readonly AppDbContext _context;

        public HealthController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Basic health check endpoint
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                // Check database connectivity
                var canConnect = await _context.Database.CanConnectAsync();
                
                if (canConnect)
                {
                    return Ok(new
                    {
                        status = "healthy",
                        timestamp = DateTime.UtcNow,
                        database = "connected",
                        version = "1.0.0"
                    });
                }
                else
                {
                    return StatusCode(503, new
                    {
                        status = "unhealthy",
                        timestamp = DateTime.UtcNow,
                        database = "disconnected",
                        error = "Cannot connect to database"
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(503, new
                {
                    status = "unhealthy",
                    timestamp = DateTime.UtcNow,
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Detailed health check with database stats
        /// </summary>
        [HttpGet("detailed")]
        public async Task<IActionResult> Detailed()
        {
            try
            {
                var userCount = await _context.AspNetUsers.CountAsync();
                var adminCount = await _context.AspNetUsers.CountAsync(u => u.IsAdmin == true);
                var teacherCount = userCount - adminCount;

                return Ok(new
                {
                    status = "healthy",
                    timestamp = DateTime.UtcNow,
                    database = new
                    {
                        connected = true,
                        users = userCount,
                        admins = adminCount,
                        teachers = teacherCount
                    },
                    environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown",
                    version = "1.0.0"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(503, new
                {
                    status = "unhealthy",
                    timestamp = DateTime.UtcNow,
                    error = ex.Message
                });
            }
        }
    }
}
