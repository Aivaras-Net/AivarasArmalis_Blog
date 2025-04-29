using Blog.Models;

namespace Blog.Services.Comments.Interfaces
{
    /// <summary>
    /// Interface for reading comment data
    /// </summary>
    public interface ICommentReader
    {
        Task<List<Comment>> GetArticleCommentsAsync(int articleId, bool includeBlocked = false);
        Task<Comment?> GetCommentByIdAsync(int id);
        Task<List<Comment>> GetRepliesAsync(int commentId, bool includeBlocked = false);
    }
}