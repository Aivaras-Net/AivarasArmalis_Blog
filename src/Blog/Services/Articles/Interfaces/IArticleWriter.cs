using Blog.Models;

namespace Blog.Services.Articles.Interfaces
{
    /// <summary>
    /// Handles write operations for articles
    /// </summary>
    public interface IArticleWriter
    {
        /// <summary>
        /// Creates a new article
        /// </summary>
        Task<Article?> CreateArticleAsync(Article article, string userId);

        /// <summary>
        /// Updates an existing article
        /// </summary>
        Task<Article?> UpdateArticleAsync(Article article, string userId, bool isAdmin);

        /// <summary>
        /// Deletes an article
        /// </summary>
        Task<bool> DeleteArticleAsync(int id);
    }
}