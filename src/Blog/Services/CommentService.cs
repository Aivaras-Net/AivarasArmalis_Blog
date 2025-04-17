using Blog.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Blog.Services
{
    public interface ICommentService
    {
        Task<List<Comment>> GetArticleCommentsAsync(int articleId);
        Task<Comment?> GetCommentByIdAsync(int id);
        Task<Comment?> CreateCommentAsync(Comment comment, string userId);
        Task<Comment?> UpdateCommentAsync(int id, string content, string userId, bool isAdmin);
        Task<bool> DeleteCommentAsync(int id, string userId, bool isAdmin);
        Task<List<Comment>> GetRepliesAsync(int commentId);
    }

    public class CommentService : ICommentService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<CommentService> _logger;

        public CommentService(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            ILogger<CommentService> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<List<Comment>> GetArticleCommentsAsync(int articleId)
        {
            return await _context.Comments
                .Include(c => c.Author)
                .Include(c => c.Replies)
                    .ThenInclude(r => r.Author)
                .Where(c => c.ArticleId == articleId && c.ParentCommentId == null)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<Comment?> GetCommentByIdAsync(int id)
        {
            return await _context.Comments
                .Include(c => c.Author)
                .Include(c => c.Replies)
                    .ThenInclude(r => r.Author)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Comment?> CreateCommentAsync(Comment comment, string userId)
        {
            try
            {
                _logger.LogInformation("Creating comment for article {ArticleId}", comment.ArticleId);

                comment.AuthorId = userId;
                comment.CreatedAt = DateTime.Now;

                _context.Comments.Add(comment);
                await _context.SaveChangesAsync();

                var createdComment = await GetCommentByIdAsync(comment.Id);

                _logger.LogInformation("Comment {CommentId} created successfully", comment.Id);
                return createdComment;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating comment for article {ArticleId}", comment.ArticleId);
                return null;
            }
        }

        public async Task<Comment?> UpdateCommentAsync(int id, string content, string userId, bool isAdmin)
        {
            try
            {
                var comment = await _context.Comments.FindAsync(id);

                if (comment == null)
                {
                    _logger.LogWarning("Comment {CommentId} not found", id);
                    return null;
                }

                if (!isAdmin && comment.AuthorId != userId)
                {
                    _logger.LogWarning("Unauthorized edit attempt on comment {CommentId}", id);
                    return null;
                }

                comment.Content = content;
                comment.UpdatedAt = DateTime.Now;

                _context.Update(comment);
                await _context.SaveChangesAsync();

                var updatedComment = await GetCommentByIdAsync(comment.Id);

                _logger.LogInformation("Comment {CommentId} updated successfully", id);
                return updatedComment;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating comment {CommentId}", id);
                return null;
            }
        }

        public async Task<bool> DeleteCommentAsync(int id, string userId, bool isAdmin)
        {
            try
            {
                var comment = await _context.Comments
                    .Include(c => c.Replies)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (comment == null)
                {
                    return false;
                }

                if (!isAdmin && comment.AuthorId != userId)
                {
                    _logger.LogWarning("Unauthorized delete attempt on comment {CommentId}", id);
                    return false;
                }

                _context.Comments.Remove(comment);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Comment {CommentId} deleted successfully", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting comment {CommentId}", id);
                return false;
            }
        }

        public async Task<List<Comment>> GetRepliesAsync(int commentId)
        {
            return await _context.Comments
                .Include(c => c.Author)
                .Where(c => c.ParentCommentId == commentId)
                .OrderBy(c => c.CreatedAt)
                .ToListAsync();
        }
    }
}