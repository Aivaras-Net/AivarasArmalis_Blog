using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Blog.Models
{
    public class Article
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; } = string.Empty;

        [StringLength(200)]
        public string? Summary { get; set; }

        public string? Content { get; set; }

        [StringLength(500)]
        public string? ImageUrl { get; set; }

        public DateTime PublishedDate { get; set; } = DateTime.Now;

        public DateTime? LastUpdated { get; set; }

        [Required]
        public string AuthorId { get; set; } = string.Empty;

        public ApplicationUser? Author { get; set; }
        public List<Vote> Votes { get; set; } = new List<Vote>();

        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

        [NotMapped]
        public int VoteScore => Votes?.Sum(v => v.IsUpvote ? 1 : -1) ?? 0;

        [NotMapped]
        public int UpvoteCount => Votes?.Count(v => v.IsUpvote) ?? 0;

        [NotMapped]
        public int DownvoteCount => Votes?.Count(v => !v.IsUpvote) ?? 0;
    }
}