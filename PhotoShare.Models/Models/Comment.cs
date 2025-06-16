using System.ComponentModel.DataAnnotations;

namespace PhotoShare.Models
{
    public class Comment
    {
        public int Id { get; set; }
        
        [Required]
        [MaxLength(500)]
        public string Content { get; set; } = string.Empty;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Foreign keys
        public int UserId { get; set; }
        public virtual User User { get; set; } = null!;
        
        public int PhotoId { get; set; }
        public virtual Photo Photo { get; set; } = null!;
    }
}