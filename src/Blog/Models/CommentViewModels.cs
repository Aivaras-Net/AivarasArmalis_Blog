using System.ComponentModel.DataAnnotations;

namespace Blog.Models
{
    public class ReportCommentViewModel
    {
        [Required]
        public int CommentId { get; set; }

        [Required]
        [MinLength(10, ErrorMessage = "Please provide a more detailed reason for reporting this comment")]
        [MaxLength(500, ErrorMessage = "Reason cannot exceed 500 characters")]
        public string Reason { get; set; } = string.Empty;
    }

    public class ReviewReportViewModel
    {
        [Required]
        public int ReportId { get; set; }

        [Required]
        public ReportStatus Status { get; set; }

        [MaxLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
        public string? Notes { get; set; }
    }

    public class BlockCommentViewModel
    {
        [Required]
        public int CommentId { get; set; }

        [Required]
        [MinLength(5, ErrorMessage = "Please provide a more detailed reason for blocking this comment")]
        [MaxLength(200, ErrorMessage = "Reason cannot exceed 200 characters")]
        public string Reason { get; set; } = string.Empty;
    }
}