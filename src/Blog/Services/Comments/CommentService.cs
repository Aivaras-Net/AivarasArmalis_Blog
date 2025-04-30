using Blog.Models;
using Blog.Services.Comments.Interfaces;

namespace Blog.Services.Comments
{
    /// <summary>
    /// Combined service implementation for backward compatibility
    /// </summary>
    public class CommentService : ICommentService
    {
        private readonly ICommentReader _commentReader;
        private readonly ICommentManager _commentManager;

        public CommentService(
            ICommentReader commentReader,
            ICommentManager commentManager)
        {
            _commentReader = commentReader;
            _commentManager = commentManager;
        }

        public Task<List<Comment>> GetArticleCommentsAsync(int articleId, bool includeBlocked = false) =>
            _commentReader.GetArticleCommentsAsync(articleId, includeBlocked);

        public Task<Comment?> GetCommentByIdAsync(int id) =>
            _commentReader.GetCommentByIdAsync(id);

        public Task<List<Comment>> GetRepliesAsync(int commentId, bool includeBlocked = false) =>
            _commentReader.GetRepliesAsync(commentId, includeBlocked);

        public Task<Comment?> CreateCommentAsync(Comment comment, string userId) =>
            _commentManager.CreateCommentAsync(comment, userId);

        public Task<Comment?> UpdateCommentAsync(int id, string content, string userId, bool isAdmin) =>
            _commentManager.UpdateCommentAsync(id, content, userId, isAdmin);

        public Task<bool> DeleteCommentAsync(int id, string userId, bool isAdmin) =>
            _commentManager.DeleteCommentAsync(id, userId, isAdmin);
    }
}