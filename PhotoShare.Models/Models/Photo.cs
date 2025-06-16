using System.ComponentModel.DataAnnotations;

namespace PhotoShare.Models
{
    public class Photo
    {
        public int Id { get; set; }
        
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;
        
        [MaxLength(1000)]
        public string? Description { get; set; }
        
        [Required]
        [MaxLength(255)]
        public string FileName { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(500)]
        public string FilePath { get; set; } = string.Empty;
        
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
        
        public bool IsPublic { get; set; } = true;
        
        // Foreign key
        public int UserId { get; set; }
        public virtual User User { get; set; } = null!;
        
        // Navigation properties
        public virtual ICollection<Like> Likes { get; set; } = new List<Like>();
        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
    }
}