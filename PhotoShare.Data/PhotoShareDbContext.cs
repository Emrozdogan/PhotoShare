using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PhotoShare.Models;

namespace PhotoShare.Data
{
    public class PhotoShareDbContext : IdentityDbContext<User, IdentityRole<int>, int>
    {
        public PhotoShareDbContext(DbContextOptions<PhotoShareDbContext> options) : base(options)
        {
        }

        public DbSet<Photo> Photos { get; set; }
        public DbSet<Like> Likes { get; set; }
        public DbSet<Comment> Comments { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure Like entity for composite key
            builder.Entity<Like>()
                .HasIndex(l => new { l.UserId, l.PhotoId })
                .IsUnique();

            // Configure relationships
            builder.Entity<Photo>()
                .HasOne(p => p.User)
                .WithMany(u => u.Photos)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Like>()
                .HasOne(l => l.User)
                .WithMany(u => u.Likes)
                .HasForeignKey(l => l.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Like>()
                .HasOne(l => l.Photo)
                .WithMany(p => p.Likes)
                .HasForeignKey(l => l.PhotoId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Comment>()
                .HasOne(c => c.User)
                .WithMany(u => u.Comments)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Comment>()
                .HasOne(c => c.Photo)
                .WithMany(p => p.Comments)
                .HasForeignKey(c => c.PhotoId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}