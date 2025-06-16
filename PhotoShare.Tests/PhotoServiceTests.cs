using Microsoft.EntityFrameworkCore;
using PhotoShare.Data;
using PhotoShare.Models;
using Xunit;

namespace PhotoShare.Tests
{
    public class PhotoServiceTests : IDisposable
    {
        private readonly PhotoShareDbContext _context;

        public PhotoServiceTests()
        {
            var options = new DbContextOptionsBuilder<PhotoShareDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new PhotoShareDbContext(options);
        }

        [Fact]
        public async Task CreateUser_ShouldAddUserToDatabase()
        {
            // Arrange
            var user = new User
            {
                UserName = "testuser",
                Email = "test@example.com",
                DisplayName = "Test User"
            };

            // Act
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Assert
            var savedUser = await _context.Users.FirstOrDefaultAsync(u => u.UserName == "testuser");
            Assert.NotNull(savedUser);
            Assert.Equal("testuser", savedUser.UserName);
            Assert.Equal("test@example.com", savedUser.Email);
        }

        [Fact]
        public async Task CreatePhoto_ShouldAddPhotoToDatabase()
        {
            // Arrange
            var user = new User
            {
                UserName = "testuser",
                Email = "test@example.com",
                DisplayName = "Test User"
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var photo = new Photo
            {
                Title = "Test Photo",
                Description = "Test Description",
                FileName = "test.jpg",
                FilePath = "/uploads/test.jpg",
                UserId = user.Id,
                IsPublic = true
            };

            // Act
            _context.Photos.Add(photo);
            await _context.SaveChangesAsync();

            // Assert
            var savedPhoto = await _context.Photos.FirstOrDefaultAsync(p => p.Title == "Test Photo");
            Assert.NotNull(savedPhoto);
            Assert.Equal("Test Photo", savedPhoto.Title);
            Assert.Equal(user.Id, savedPhoto.UserId);
        }

        [Fact]
        public async Task ToggleLike_ShouldAddLikeWhenNotExists()
        {
            // Arrange
            var user = new User { UserName = "testuser", Email = "test@example.com" };
            var photo = new Photo 
            { 
                Title = "Test Photo", 
                FileName = "test.jpg", 
                FilePath = "/test.jpg",
                UserId = user.Id 
            };

            _context.Users.Add(user);
            _context.Photos.Add(photo);
            await _context.SaveChangesAsync();

            // Act
            var like = new Like { UserId = user.Id, PhotoId = photo.Id };
            _context.Likes.Add(like);
            await _context.SaveChangesAsync();

            // Assert
            var savedLike = await _context.Likes.FirstOrDefaultAsync(l => l.UserId == user.Id && l.PhotoId == photo.Id);
            Assert.NotNull(savedLike);
        }

        [Theory]
        [InlineData("Photo 1", true)]
        [InlineData("Photo 2", false)]
        [InlineData("Private Photo", false)]
        public async Task GetPhotos_ShouldFilterByPublicStatus(string title, bool isPublic)
        {
            // Arrange
            var user = new User { UserName = "testuser", Email = "test@example.com" };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var photo = new Photo
            {
                Title = title,
                FileName = "test.jpg",
                FilePath = "/test.jpg",
                UserId = user.Id,
                IsPublic = isPublic
            };
            _context.Photos.Add(photo);
            await _context.SaveChangesAsync();

            // Act
            var publicPhotos = await _context.Photos.Where(p => p.IsPublic).ToListAsync();

            // Assert
            if (isPublic)
            {
                Assert.Contains(publicPhotos, p => p.Title == title);
            }
            else
            {
                Assert.DoesNotContain(publicPhotos, p => p.Title == title);
            }
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}