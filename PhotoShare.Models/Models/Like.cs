namespace PhotoShare.Models
{
    public class Like
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Foreign keys
        public int UserId { get; set; }
        public virtual User User { get; set; } = null!;
        
        public int PhotoId { get; set; }
        public virtual Photo Photo { get; set; } = null!;
    }
}