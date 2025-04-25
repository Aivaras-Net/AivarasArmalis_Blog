using System.Collections.Generic;

namespace Blog.Models
{
    public class HomeViewModel
    {
        public List<Article> RecentArticles { get; set; } = new List<Article>();
        public List<Article> TopRankedArticles { get; set; } = new List<Article>();
        public List<Article> RecentlyCommentedArticles { get; set; } = new List<Article>();
    }
}