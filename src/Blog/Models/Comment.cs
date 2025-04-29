using System.ComponentModel.DataAnnotations;

namespace Blog.Models
{
    public class Comment
    {
        public int Id { get; set; }

        [Required]
        public string Content { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }

        [Required]
        public string AuthorId { get; set; } = string.Empty;

        public virtual ApplicationUser? Author { get; set; }

        [Required]
        public int ArticleId { get; set; }

        public virtual Article? Article { get; set; }

        public int? ParentCommentId { get; set; }

        public virtual Comment? ParentComment { get; set; }

        public virtual ICollection<Comment> Replies { get; set; } = new List<Comment>();

        public bool IsBlocked { get; set; } = false;

        public DateTime? BlockedAt { get; set; }

        public string? BlockedById { get; set; }

        public virtual ApplicationUser? BlockedBy { get; set; }

        public string? BlockReason { get; set; }

        public virtual ICollection<CommentReport> Reports { get; set; } = new List<CommentReport>();
    }
}