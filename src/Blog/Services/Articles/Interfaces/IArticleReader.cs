using Blog.Models;

namespace Blog.Services.Articles.Interfaces
{
    /// <summary>
    /// Handles reading article data
    /// </summary>
    public interface IArticleReader
    {
        /// <summary>
        /// Gets all articles with their related data
        /// </summary>
        Task<List<Article>> GetAllArticlesAsync();

        /// <summary>
        /// Gets a specific article by its ID with related data
        /// </summary>
        Task<Article?> GetArticleByIdAsync(int? id);

        /// <summary>
        /// Checks if an article exists
        /// </summary>
        bool ArticleExists(int id);
    }
}