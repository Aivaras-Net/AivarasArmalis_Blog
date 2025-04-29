using System.ComponentModel.DataAnnotations;

namespace Blog.Models
{
    public enum ReportStatus
    {
        Pending,
        Reviewed,
        Rejected,
        ActionTaken
    }

    public class CommentReport
    {
        public int Id { get; set; }

        [Required]
        public int CommentId { get; set; }
        public virtual Comment? Comment { get; set; }

        [Required]
        public string ReporterId { get; set; } = string.Empty;
        public virtual ApplicationUser? Reporter { get; set; }

        [Required]
        [MaxLength(500)]
        public string Reason { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string? ReportDetails { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public ReportStatus Status { get; set; } = ReportStatus.Pending;

        public DateTime? ReviewedAt { get; set; }

        public string? ReviewerId { get; set; }
        public virtual ApplicationUser? Reviewer { get; set; }

        public string? ReviewNotes { get; set; }
    }
}