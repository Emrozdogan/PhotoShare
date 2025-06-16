using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace PhotoShare.Models
{
    public class User : IdentityUser<int>
    {
        [MaxLength(100)]
        public string? DisplayName { get; set; }
        
        [MaxLength(500)]
        public string? ProfilePictureUrl { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public bool IsAdmin { get; set; } = false;
        
        // Navigation properties
        public virtual ICollection<Photo> Photos { get; set; } = new List<Photo>();
        public virtual ICollection<Like> Likes { get; set; } = new List<Like>();
        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
    }
}