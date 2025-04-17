using System.ComponentModel.DataAnnotations;

namespace Blog.Models
{
    public class Vote
    {
        public int Id { get; set; }

        [Required]
        public int ArticleId { get; set; }
        public Article? Article { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser? User { get; set; }

        [Required]
        public bool IsUpvote { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}