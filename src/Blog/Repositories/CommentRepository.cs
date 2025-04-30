using Blog.Models;
using Microsoft.EntityFrameworkCore;

namespace Blog.Repositories
{
    /// <summary>
    /// Repository interface for comment data access
    /// </summary>
    public interface ICommentRepository
    {
        Task<List<Comment>> GetArticleCommentsAsync(int articleId);
        Task<Comment?> GetByIdAsync(int id, bool includeRelated = true);
        Task<Comment> AddAsync(Comment comment);
        Task<Comment?> UpdateAsync(Comment comment);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<List<Comment>> GetRepliesAsync(int commentId);
    }

    /// <summary>
    /// Repository implementation for comment data access
    /// </summary>
    public class CommentRepository : ICommentRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CommentRepository> _logger;

        public CommentRepository(
            ApplicationDbContext context,
            ILogger<CommentRepository> logger)
        {
            _context = context;
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

        public async Task<Comment?> GetByIdAsync(int id, bool includeRelated = true)
        {
            IQueryable<Comment> query = _context.Comments;

            if (includeRelated)
            {
                query = query
                    .Include(c => c.Author)
                    .Include(c => c.Replies)
                        .ThenInclude(r => r.Author)
                    .Include(c => c.BlockedBy);
            }

            return await query.FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Comment> AddAsync(Comment comment)
        {
            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();
            return comment;
        }

        public async Task<Comment?> UpdateAsync(Comment comment)
        {
            try
            {
                _context.Comments.Update(comment);
                await _context.SaveChangesAsync();
                return comment;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating comment {CommentId}", comment.Id);
                return null;
            }
        }

        public async Task<bool> DeleteAsync(int id)
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

                _context.Comments.Remove(comment);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting comment {CommentId}", id);
                return false;
            }
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Comments.AnyAsync(c => c.Id == id);
        }

        public async Task<List<Comment>> GetRepliesAsync(int commentId)
        {
            var replies = await _context.Comments
                .Include(c => c.Author)
                .Where(c => c.ParentCommentId == commentId)
                .OrderBy(c => c.CreatedAt)
                .ToListAsync();

            foreach (var reply in replies)
            {
                await LoadNestedRepliesAsync(reply);
            }

            return replies;
        }

        private async Task LoadNestedRepliesAsync(Comment comment)
        {
            var replies = await _context.Comments
                .Include(c => c.Author)
                .Where(c => c.ParentCommentId == comment.Id)
                .OrderBy(c => c.CreatedAt)
                .ToListAsync();

            comment.Replies = replies;

            foreach (var reply in replies)
            {
                await LoadNestedRepliesAsync(reply);
            }
        }
    }
}