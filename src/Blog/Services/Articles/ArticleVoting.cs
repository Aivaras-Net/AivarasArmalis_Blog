using Blog.Models;
using Blog.Repositories;

namespace Blog.Services.Articles
{
    /// <summary>
    /// Implementation of article voting operations
    /// </summary>
    public class ArticleVoting : IArticleVoting
    {
        private readonly IArticleRepository _repository;
        private readonly ILogger<ArticleVoting> _logger;

        public ArticleVoting(
            IArticleRepository repository,
            ILogger<ArticleVoting> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<bool> VoteAsync(int articleId, string userId, bool isUpvote)
        {
            _logger.LogInformation("Processing vote for article {ArticleId} by user {UserId}, isUpvote: {IsUpvote}",
                articleId, userId, isUpvote);

            var vote = new Vote
            {
                ArticleId = articleId,
                UserId = userId,
                IsUpvote = isUpvote
            };

            return await _repository.AddOrUpdateVoteAsync(vote);
        }

        public async Task<bool> RemoveVoteAsync(int articleId, string userId)
        {
            _logger.LogInformation("Removing vote for article {ArticleId} by user {UserId}",
                articleId, userId);

            return await _repository.RemoveVoteAsync(articleId, userId);
        }

        public async Task<Vote?> GetUserVoteAsync(int articleId, string userId)
        {
            return await _repository.GetVoteAsync(articleId, userId);
        }
    }
}