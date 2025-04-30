using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

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
    }

    /// <summary>
    /// Author information DTO
    /// </summary>
    public class AuthorDto
    {
        public string Name { get; set; } = string.Empty;
    }

    /// <summary>
    /// Simplified DTO for article list view with minimal information
    /// </summary>
    public class ArticleBriefDto
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string? Summary { get; set; }

        public string? ImageUrl { get; set; }

        public DateTime PublishedDate { get; set; }

        public AuthorDto Author { get; set; } = new AuthorDto();
    }

    /// <summary>
    /// Voting statistics DTO
    /// </summary>
    public class VoteStatsDto
    {
        public int Score { get; set; }
        public int UpvoteCount { get; set; }
        public int DownvoteCount { get; set; }
        public bool? CurrentUserVoted { get; set; }
        public bool? CurrentUserVotedUp { get; set; }
    }

    /// <summary>
    /// DTO for displaying article in a list view
    /// </summary>
    public class ArticleListItemDto : ArticleBaseDto
    {
        [JsonPropertyOrder(1000)]
        public string? Content { get; set; }

        [JsonPropertyOrder(1001)]
        public AuthorDto Author { get; set; } = new AuthorDto();

        [JsonPropertyOrder(1002)]
        public VoteStatsDto Votes { get; set; } = new VoteStatsDto();
    }

    /// <summary>
    /// DTO for full article details
    /// </summary>
    public class ArticleDetailDto : ArticleListItemDto
    {
        [JsonPropertyOrder(1003)]
        public ICollection<CommentDto> Comments { get; set; } = new List<CommentDto>();
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
        [JsonIgnore]
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

        public AuthorDto Author { get; set; } = new AuthorDto();

        public int? ParentCommentId { get; set; }

        public ICollection<CommentDto> Replies { get; set; } = new List<CommentDto>();
    }
}