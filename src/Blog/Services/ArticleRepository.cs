using Blog.Models;
using Microsoft.EntityFrameworkCore;

namespace Blog.Services
{
    /// <summary>
    /// Repository interface for article data access
    /// </summary>
    public interface IArticleRepository
    {
        Task<List<Article>> GetAllAsync(bool includeAuthor = true, bool includeVotes = true);
        Task<Article?> GetByIdAsync(int id, bool includeAuthor = true, bool includeVotes = true);
        Task<Article> AddAsync(Article article);
        Task<Article?> UpdateAsync(Article article);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<Vote?> GetVoteAsync(int articleId, string userId);
        Task<bool> AddOrUpdateVoteAsync(Vote vote);
        Task<bool> RemoveVoteAsync(int articleId, string userId);
    }

    /// <summary>
    /// Repository implementation for article data access
    /// </summary>
    public class ArticleRepository : IArticleRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ArticleRepository> _logger;

        public ArticleRepository(
            ApplicationDbContext context,
            ILogger<ArticleRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<Article>> GetAllAsync(bool includeAuthor = true, bool includeVotes = true)
        {
            IQueryable<Article> query = _context.Articles;

            if (includeAuthor)
            {
                query = query.Include(a => a.Author);
            }

            if (includeVotes)
            {
                query = query.Include(a => a.Votes);
            }

            return await query.ToListAsync();
        }

        public async Task<Article?> GetByIdAsync(int id, bool includeAuthor = true, bool includeVotes = true)
        {
            IQueryable<Article> query = _context.Articles;

            if (includeAuthor)
            {
                query = query.Include(a => a.Author);
            }

            if (includeVotes)
            {
                query = query.Include(a => a.Votes);
            }

            return await query.FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<Article> AddAsync(Article article)
        {
            _context.Articles.Add(article);
            await _context.SaveChangesAsync();
            return article;
        }

        public async Task<Article?> UpdateAsync(Article article)
        {
            try
            {
                _context.Articles.Update(article);
                await _context.SaveChangesAsync();
                return article;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating article {ArticleId}", article.Id);
                return null;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var article = await _context.Articles.FindAsync(id);
                if (article == null)
                {
                    return false;
                }

                _context.Articles.Remove(article);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting article {ArticleId}", id);
                return false;
            }
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Articles.AnyAsync(e => e.Id == id);
        }

        public async Task<Vote?> GetVoteAsync(int articleId, string userId)
        {
            return await _context.Votes
                .FirstOrDefaultAsync(v => v.ArticleId == articleId && v.UserId == userId);
        }

        public async Task<bool> AddOrUpdateVoteAsync(Vote vote)
        {
            try
            {
                var existingVote = await GetVoteAsync(vote.ArticleId, vote.UserId);

                if (existingVote != null)
                {
                    existingVote.IsUpvote = vote.IsUpvote;
                    _context.Votes.Update(existingVote);
                }
                else
                {
                    _context.Votes.Add(vote);
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding/updating vote for article {ArticleId}", vote.ArticleId);
                return false;
            }
        }

        public async Task<bool> RemoveVoteAsync(int articleId, string userId)
        {
            try
            {
                var vote = await GetVoteAsync(articleId, userId);
                if (vote == null)
                {
                    return false;
                }

                _context.Votes.Remove(vote);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing vote for article {ArticleId}", articleId);
                return false;
            }
        }
    }
}