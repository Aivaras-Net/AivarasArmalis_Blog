using System;
using System.ComponentModel.DataAnnotations;

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
    }
}