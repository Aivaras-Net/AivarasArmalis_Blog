using Blog.Models;

namespace Blog.Services.Comments.Interfaces
{
    /// <summary>
    /// Interface for managing comments (create, update, delete)
    /// </summary>
    public interface ICommentManager
    {
        Task<Comment?> CreateCommentAsync(Comment comment, string userId);
        Task<Comment?> UpdateCommentAsync(int id, string content, string userId, bool isAdmin);
        Task<bool> DeleteCommentAsync(int id, string userId, bool isAdmin);
    }
}