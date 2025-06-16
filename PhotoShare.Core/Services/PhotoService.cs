using PhotoShare.Data;  // This line is crucial
using PhotoShare.Models;
using PhotoShare.Models.DTOs;
using Microsoft.EntityFrameworkCore;

namespace PhotoShare.Core.Services
{
    public interface IPhotoService
    {
        Task<IEnumerable<PhotoDto>> GetPublicPhotosAsync(int page = 1, int pageSize = 20);
        Task<PhotoStatsDto> GetPhotoStatisticsAsync();
        Task<IEnumerable<PhotoDto>> SearchPhotosAsync(string searchTerm);
    }

    public class PhotoService : IPhotoService
    {
        private readonly PhotoShareDbContext _context;

        public PhotoService(PhotoShareDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<PhotoDto>> GetPublicPhotosAsync(int page = 1, int pageSize = 20)
        {
            return await _context.Photos
                .Where(p => p.IsPublic)
                .Include(p => p.User)
                .Include(p => p.Likes)
                .Include(p => p.Comments)
                .OrderByDescending(p => p.UploadedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new PhotoDto
                {
                    Id = p.Id,
                    Title = p.Title,
                    Description = p.Description,
                    FilePath = p.FilePath,
                    UploadedAt = p.UploadedAt,
                    UserName = p.User.UserName ?? "Unknown",
                    LikesCount = p.Likes.Count(),
                    CommentsCount = p.Comments.Count()
                })
                .ToListAsync();
        }

        public async Task<PhotoStatsDto> GetPhotoStatisticsAsync()
{
    var totalPhotos = await _context.Photos.CountAsync();
    var totalLikes = await _context.Likes.CountAsync();
    
    var mostLikedPhoto = await _context.Photos
        .Include(p => p.Likes)
        .Include(p => p.User)
        .OrderByDescending(p => p.Likes.Count())
        .Select(p => new { p.Title, LikeCount = p.Likes.Count(), UserName = p.User.UserName ?? "Unknown" })
        .FirstOrDefaultAsync();

    var userStats = await _context.Users
        .Where(u => u.UserName != null) // Filter out null usernames
        .Select(u => new
        {
            UserName = u.UserName!, // Tell compiler it's not null
            PhotoCount = u.Photos.Count(),
            TotalLikes = u.Photos.SelectMany(p => p.Likes).Count()
        })
        .Where(u => u.PhotoCount > 0)
        .OrderByDescending(u => u.TotalLikes)
        .Take(5)
        .ToListAsync();

    return new PhotoStatsDto
    {
        TotalPhotos = totalPhotos,
        TotalLikes = totalLikes,
        MostLikedPhoto = mostLikedPhoto?.Title ?? "None",
        MostLikedPhotoLikes = mostLikedPhoto?.LikeCount ?? 0,
        TopUsers = userStats.Select(u => u.UserName).ToList() // Now guaranteed to be non-null
    };
}

        public async Task<IEnumerable<PhotoDto>> SearchPhotosAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return Enumerable.Empty<PhotoDto>();

            var searchWords = searchTerm.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);

            return await _context.Photos
                .Where(p => p.IsPublic && 
                       searchWords.Any(word => 
                           p.Title.ToLower().Contains(word) || 
                           (p.Description != null && p.Description.ToLower().Contains(word))))
                .Include(p => p.User)
                .Include(p => p.Likes)
                .Select(p => new PhotoDto
                {
                    Id = p.Id,
                    Title = p.Title,
                    Description = p.Description,
                    FilePath = p.FilePath,
                    UploadedAt = p.UploadedAt,
                    UserName = p.User.UserName ?? "Unknown",
                    LikesCount = p.Likes.Count()
                })
                .ToListAsync();
        }
    }
}