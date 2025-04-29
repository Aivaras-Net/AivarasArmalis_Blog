using Blog.Models;

namespace Blog.Services.Articles.Interfaces
{

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