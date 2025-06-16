using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhotoShare.Data;
using PhotoShare.Models;

namespace PhotoShare.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PhotosController : ControllerBase
    {
        private readonly PhotoShareDbContext _context;

        public PhotosController(PhotoShareDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetPhotos()
        {
            var photos = await _context.Photos
                .Include(p => p.User)
                .Include(p => p.Likes)
                .Where(p => p.IsPublic)
                .OrderByDescending(p => p.UploadedAt)
                .Select(p => new
                {
                    p.Id,
                    p.Title,
                    p.Description,
                    p.FilePath,
                    p.UploadedAt,
                    UserName = p.User.UserName,
                    LikesCount = p.Likes.Count()
                })
                .ToListAsync();

            return Ok(photos);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetPhoto(int id)
        {
            var photo = await _context.Photos
                .Include(p => p.User)
                .Include(p => p.Likes)
                .Include(p => p.Comments)
                .Where(p => p.Id == id)
                .Select(p => new
                {
                    p.Id,
                    p.Title,
                    p.Description,
                    p.FilePath,
                    p.UploadedAt,
                    UserName = p.User.UserName,
                    LikesCount = p.Likes.Count(),
                    CommentsCount = p.Comments.Count()
                })
                .FirstOrDefaultAsync();

            if (photo == null)
            {
                return NotFound();
            }

            return Ok(photo);
        }

        [HttpPost("test")]
        public async Task<ActionResult> CreateTestPhoto()
        {
            // Create a test user first
            var user = new User
            {
                UserName = "testuser",
                Email = "test@example.com",
                DisplayName = "Test User"
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Create a test photo
            var photo = new Photo
            {
                Title = "Test Photo",
                Description = "This is a test photo",
                FileName = "test.jpg",
                FilePath = "/uploads/test.jpg",
                UserId = user.Id,
                IsPublic = true
            };

            _context.Photos.Add(photo);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Test photo created successfully", photoId = photo.Id });
        }
    }
}