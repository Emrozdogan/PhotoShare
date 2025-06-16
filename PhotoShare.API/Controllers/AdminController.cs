using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhotoShare.Data;
using PhotoShare.Models;

namespace PhotoShare.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly PhotoShareDbContext _context;

        public AdminController(PhotoShareDbContext context)
        {
            _context = context;
        }

        [HttpPost("setup")]
        public async Task<ActionResult> SetupApplication()
        {
            try
            {
                // Create admin user if doesn't exist
                var adminUser = await _context.Users.FirstOrDefaultAsync(u => u.UserName == "admin");
                if (adminUser == null)
                {
                    adminUser = new User
                    {
                        UserName = "admin",
                        Email = "admin@photoshare.com",
                        DisplayName = "Administrator",
                        IsAdmin = true
                    };
                    _context.Users.Add(adminUser);
                }

                // Create sample users if database is empty
                var userCount = await _context.Users.CountAsync();
                if (userCount <= 1) // Only admin exists
                {
                    var sampleUsers = new[]
                    {
                        new User { UserName = "photographer1", Email = "photo1@example.com", DisplayName = "John Photographer" },
                        new User { UserName = "nature_lover", Email = "nature@example.com", DisplayName = "Sarah Nature" },
                        new User { UserName = "travel_bug", Email = "travel@example.com", DisplayName = "Mike Traveler" }
                    };

                    _context.Users.AddRange(sampleUsers);
                }

                await _context.SaveChangesAsync();

                return Ok(new { 
                    message = "Application setup completed successfully",
                    adminCreated = true,
                    sampleUsersCreated = userCount <= 1
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Setup failed: {ex.Message}");
            }
        }

        [HttpGet("stats")]
        public async Task<ActionResult> GetApplicationStats()
        {
            try
            {
                var totalUsers = await _context.Users.CountAsync();
                var totalPhotos = await _context.Photos.CountAsync();
                var totalLikes = await _context.Likes.CountAsync();
                var totalComments = await _context.Comments.CountAsync();

                var recentPhotos = await _context.Photos
                    .Include(p => p.User)
                    .OrderByDescending(p => p.UploadedAt)
                    .Take(5)
                    .Select(p => new
                    {
                        p.Id,
                        p.Title,
                        p.UploadedAt,
                        UserName = p.User.UserName
                    })
                    .ToListAsync();

                var topUsers = await _context.Users
                    .Include(u => u.Photos)
                    .OrderByDescending(u => u.Photos.Count)
                    .Take(5)
                    .Select(u => new
                    {
                        u.UserName,
                        u.DisplayName,
                        PhotoCount = u.Photos.Count,
                        u.IsAdmin
                    })
                    .ToListAsync();

                return Ok(new
                {
                    statistics = new
                    {
                        totalUsers,
                        totalPhotos,
                        totalLikes,
                        totalComments
                    },
                    recentPhotos,
                    topUsers
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Failed to get stats: {ex.Message}");
            }
        }

        [HttpDelete("photos/{id}")]
        public async Task<ActionResult> DeletePhoto(int id, [FromQuery] string adminUser = "admin")
        {
            try
            {
                // Verify admin user
                var admin = await _context.Users.FirstOrDefaultAsync(u => u.UserName == adminUser && u.IsAdmin);
                if (admin == null)
                {
                    return Unauthorized("Admin access required");
                }

                var photo = await _context.Photos.FindAsync(id);
                if (photo == null)
                {
                    return NotFound("Photo not found");
                }

                // Remove associated likes and comments
                var likes = await _context.Likes.Where(l => l.PhotoId == id).ToListAsync();
                var comments = await _context.Comments.Where(c => c.PhotoId == id).ToListAsync();

                _context.Likes.RemoveRange(likes);
                _context.Comments.RemoveRange(comments);
                _context.Photos.Remove(photo);

                await _context.SaveChangesAsync();

                return Ok(new { message = "Photo deleted successfully by admin" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Delete failed: {ex.Message}");
            }
        }

        [HttpPut("users/{username}/admin")]
        public async Task<ActionResult> ToggleAdminStatus(string username, [FromQuery] string adminUser = "admin")
        {
            try
            {
                // Verify admin user
                var admin = await _context.Users.FirstOrDefaultAsync(u => u.UserName == adminUser && u.IsAdmin);
                if (admin == null)
                {
                    return Unauthorized("Admin access required");
                }

                var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == username);
                if (user == null)
                {
                    return NotFound("User not found");
                }

                user.IsAdmin = !user.IsAdmin;
                await _context.SaveChangesAsync();

                return Ok(new 
                { 
                    message = $"User {username} admin status updated",
                    isAdmin = user.IsAdmin
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Update failed: {ex.Message}");
            }
        }
    }
}