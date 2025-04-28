using System.ComponentModel.DataAnnotations;

namespace Blog.Models.Dtos
{
    /// <summary>
    /// Base DTO for article-related data transfer
    /// </summary>
    public class ArticleBaseDto
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; } = string.Empty;

        [StringLength(200)]
        public string? Summary { get; set; }

        [StringLength(500)]
        public string? ImageUrl { get; set; }

        public DateTime PublishedDate { get; set; }

        public DateTime? LastUpdated { get; set; }

        public int VoteScore { get; set; }

        public int UpvoteCount { get; set; }

        public int DownvoteCount { get; set; }
    }

    /// <summary>
    /// DTO for displaying article in a list view
    /// </summary>
    public class ArticleListItemDto : ArticleBaseDto
    {
        public string? AuthorName { get; set; }

        public string? AuthorId { get; set; }

        public string? AuthorProfilePicture { get; set; }
    }

    /// <summary>
    /// DTO for full article details
    /// </summary>
    public class ArticleDetailDto : ArticleListItemDto
    {
        public string? Content { get; set; }

        public ICollection<CommentDto> Comments { get; set; } = new List<CommentDto>();

        public bool? CurrentUserVoted { get; set; }

        public bool? CurrentUserVotedUp { get; set; }
    }

    /// <summary>
    /// DTO for creating a new article
    /// </summary>
    public class ArticleCreateDto
    {
        [Required]
        [StringLength(100)]
        public string Title { get; set; } = string.Empty;

        [StringLength(200)]
        public string? Summary { get; set; }

        public string? Content { get; set; }

        [StringLength(500)]
        public string? ImageUrl { get; set; }
    }

    /// <summary>
    /// DTO for updating an article
    /// </summary>
    public class ArticleUpdateDto : ArticleCreateDto
    {
        public int Id { get; set; }
    }

    /// <summary>
    /// DTO for article vote operations
    /// </summary>
    public class ArticleVoteDto
    {
        public int ArticleId { get; set; }

        public bool IsUpvote { get; set; }
    }

    /// <summary>
    /// Simplified comment DTO for article details view
    /// </summary>
    public class CommentDto
    {
        public int Id { get; set; }

        public string Content { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public string? AuthorName { get; set; }

        public string? AuthorProfilePicture { get; set; }

        public int? ParentCommentId { get; set; }

        public ICollection<CommentDto> Replies { get; set; } = new List<CommentDto>();
    }
}