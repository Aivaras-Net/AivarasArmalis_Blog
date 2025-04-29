using Blog.Models;
using Blog.Repositories;
using Blog.Services.Comments.Interfaces;

namespace Blog.Services.Comments
{
    /// <summary>
    /// Service for reading comment data
    /// </summary>
    public class CommentReader : ICommentReader
    {
        private readonly ICommentRepository _commentRepository;
        private readonly ILogger<CommentReader> _logger;

        public CommentReader(
            ICommentRepository commentRepository,
            ILogger<CommentReader> logger)
        {
            _commentRepository = commentRepository;
            _logger = logger;
        }

        public async Task<List<Comment>> GetArticleCommentsAsync(int articleId, bool includeBlocked = false)
        {
            _logger.LogInformation("Getting comments for article {ArticleId}", articleId);
            return await _commentRepository.GetArticleCommentsAsync(articleId);
        }

        public async Task<Comment?> GetCommentByIdAsync(int id)
        {
            _logger.LogInformation("Getting comment {CommentId}", id);
            return await _commentRepository.GetByIdAsync(id);
        }

        public async Task<List<Comment>> GetRepliesAsync(int commentId, bool includeBlocked = false)
        {
            _logger.LogInformation("Getting replies for comment {CommentId}", commentId);
            return await _commentRepository.GetRepliesAsync(commentId);
        }
    }
}