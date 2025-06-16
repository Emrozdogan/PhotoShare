namespace PhotoShare.Models.DTOs
{
    public class PhotoDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string FilePath { get; set; } = string.Empty;
        public DateTime UploadedAt { get; set; }
        public bool IsPublic { get; set; }
        public string UserName { get; set; } = string.Empty;
        public int LikesCount { get; set; }
        public int CommentsCount { get; set; }
        public bool IsLikedByUser { get; set; }
    }
    
    public class CreatePhotoDto
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsPublic { get; set; } = true;
        public int UserId { get; set; }
    }
    public class PhotoStatsDto
    {
        public int TotalPhotos { get; set; }
        public int TotalLikes { get; set; }
        public string MostLikedPhoto { get; set; } = string.Empty;
        public int MostLikedPhotoLikes { get; set; }
        public List<string?> TopUsers { get; set; } = new(); // Allow nulls
    }
}
