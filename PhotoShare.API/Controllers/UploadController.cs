using Microsoft.AspNetCore.Mvc;
using PhotoShare.Data;
using PhotoShare.Models;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace PhotoShare.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UploadController : ControllerBase
    {
        private readonly PhotoShareDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public UploadController(PhotoShareDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        [HttpPost("photo")]
        public async Task<ActionResult> UploadPhoto([FromForm] PhotoUploadRequest request)
        {
            try
            {
                // Validate file
                if (request.File == null || request.File.Length == 0)
                    return BadRequest("No file uploaded");

                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var fileExtension = Path.GetExtension(request.File.FileName).ToLowerInvariant();
                
                if (!allowedExtensions.Contains(fileExtension))
                    return BadRequest("Invalid file type. Only JPG, PNG, and GIF files are allowed.");

                if (request.File.Length > 5 * 1024 * 1024) // 5MB limit
                    return BadRequest("File size cannot exceed 5MB");

                // Create uploads directory if it doesn't exist
                var uploadsPath = Path.Combine(_environment.WebRootPath ?? _environment.ContentRootPath, "uploads");
                if (!Directory.Exists(uploadsPath))
                    Directory.CreateDirectory(uploadsPath);

                // Generate unique filename
                var fileName = $"{Guid.NewGuid()}{fileExtension}";
                var filePath = Path.Combine(uploadsPath, fileName);

                // Save file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await request.File.CopyToAsync(stream);
                }

                // Create user if doesn't exist (for testing)
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

                // Create photo record
                var photo = new Photo
                {
                    Title = request.Title,
                    Description = request.Description,
                    FileName = fileName,
                    FilePath = $"/uploads/{fileName}",
                    UserId = user.Id,
                    IsPublic = request.IsPublic
                };

                _context.Photos.Add(photo);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Photo uploaded successfully",
                    photoId = photo.Id,
                    fileName = fileName,
                    filePath = photo.FilePath
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("multiple")]
        public async Task<ActionResult> UploadMultiplePhotos([FromForm] MultiplePhotoUploadRequest request)
        {
            try
            {
                if (request.Files == null || !request.Files.Any())
                    return BadRequest("No files uploaded");

                var results = new List<object>();
                var uploadsPath = Path.Combine(_environment.WebRootPath ?? _environment.ContentRootPath, "uploads");
                
                if (!Directory.Exists(uploadsPath))
                    Directory.CreateDirectory(uploadsPath);

                // Create user if doesn't exist
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

                foreach (var file in request.Files)
                {
                    if (file.Length > 0)
                    {
                        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                        var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
                        
                        if (allowedExtensions.Contains(fileExtension) && file.Length <= 5 * 1024 * 1024)
                        {
                            var fileName = $"{Guid.NewGuid()}{fileExtension}";
                            var filePath = Path.Combine(uploadsPath, fileName);

                            using (var stream = new FileStream(filePath, FileMode.Create))
                            {
                                await file.CopyToAsync(stream);
                            }

                            var photo = new Photo
                            {
                                Title = Path.GetFileNameWithoutExtension(file.FileName),
                                Description = "Bulk upload",
                                FileName = fileName,
                                FilePath = $"/uploads/{fileName}",
                                UserId = user.Id,
                                IsPublic = true
                            };

                            _context.Photos.Add(photo);
                            results.Add(new { fileName, photoId = photo.Id, status = "success" });
                        }
                        else
                        {
                            results.Add(new { fileName = file.FileName, status = "failed", reason = "Invalid file type or size" });
                        }
                    }
                }

                await _context.SaveChangesAsync();
                return Ok(new { message = "Bulk upload completed", results });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }

    public class PhotoUploadRequest
    {
        [Required]
        public IFormFile File { get; set; } = null!;
        
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;
        
        [MaxLength(1000)]
        public string? Description { get; set; }
        
        [Required]
        public string UserName { get; set; } = string.Empty;
        
        public bool IsPublic { get; set; } = true;
    }

    public class MultiplePhotoUploadRequest
    {
        [Required]
        public IFormFileCollection Files { get; set; } = null!;
        
        [Required]
        public string UserName { get; set; } = string.Empty;
    }
}