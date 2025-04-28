using Blog.Models;

namespace Blog.Services
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

    /// <summary>
    /// Handles voting operations for articles
    /// </summary>
    public interface IArticleVoting
    {
        /// <summary>
        /// Adds or updates a vote on an article
        /// </summary>
        Task<bool> VoteAsync(int articleId, string userId, bool isUpvote);

        /// <summary>
        /// Removes a vote from an article
        /// </summary>
        Task<bool> RemoveVoteAsync(int articleId, string userId);

        /// <summary>
        /// Gets a user's vote on an article
        /// </summary>
        Task<Vote?> GetUserVoteAsync(int articleId, string userId);
    }
}