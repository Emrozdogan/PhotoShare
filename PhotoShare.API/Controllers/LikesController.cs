using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhotoShare.Data;
using PhotoShare.Models;

namespace PhotoShare.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LikesController : ControllerBase
    {
        private readonly PhotoShareDbContext _context;

        public LikesController(PhotoShareDbContext context)
        {
            _context = context;
        }

        [HttpPost("{photoId}/toggle")]
        public async Task<ActionResult> ToggleLike(int photoId, [FromBody] ToggleLikeRequest request)
        {
            try
            {
                // Find or create user
                var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == request.UserName);
                if (user == null)
                {
                    user = new User
                    {
                        UserName = request.UserName,
                        Email = $"{request.UserName}@example.com",
                        DisplayName = request.UserName
                    };
                    _context.Users.Add(user);
                    await _context.SaveChangesAsync();
                }

                // Check if photo exists
                var photo = await _context.Photos.FindAsync(photoId);
                if (photo == null)
                    return NotFound("Photo not found");

                // Check if like already exists
                var existingLike = await _context.Likes
                    .FirstOrDefaultAsync(l => l.PhotoId == photoId && l.UserId == user.Id);

                bool isLiked;
                if (existingLike != null)
                {
                    // Unlike
                    _context.Likes.Remove(existingLike);
                    isLiked = false;
                }
                else
                {
                    // Like
                    var like = new Like
                    {
                        PhotoId = photoId,
                        UserId = user.Id
                    };
                    _context.Likes.Add(like);
                    isLiked = true;
                }

                await _context.SaveChangesAsync();

                // Get updated like count
                var likeCount = await _context.Likes.CountAsync(l => l.PhotoId == photoId);

                return Ok(new
                {
                    isLiked,
                    likeCount,
                    message = isLiked ? "Photo liked" : "Photo unliked"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("{photoId}")]
        public async Task<ActionResult> GetPhotoLikes(int photoId)
        {
            var likes = await _context.Likes
                .Include(l => l.User)
                .Where(l => l.PhotoId == photoId)
                .Select(l => new
                {
                    userName = l.User.UserName,
                    displayName = l.User.DisplayName,
                    likedAt = l.CreatedAt
                })
                .OrderByDescending(l => l.likedAt)
                .ToListAsync();

            return Ok(new
            {
                photoId,
                likeCount = likes.Count,
                likes
            });
        }
    }

    public class ToggleLikeRequest
    {
        public string UserName { get; set; } = string.Empty;
    }
}