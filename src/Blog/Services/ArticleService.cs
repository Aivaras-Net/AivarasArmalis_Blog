using Blog.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
namespace Blog.Services
{
    public interface IArticleService
    {
        Task<List<Article>> GetAllArticlesAsync();
        Task<Article?> GetArticleByIdAsync(int? id);
        Task<Article?> CreateArticleAsync(Article article, string userId);
        Task<Article?> UpdateArticleAsync(Article article, string userId, bool isAdmin);
        Task<bool> DeleteArticleAsync(int id);
        Task<bool> VoteAsync(int articleId, string userId, bool isUpvote);
        Task<bool> RemoveVoteAsync(int articleId, string userId);
        Task<Vote?> GetUserVoteAsync(int articleId, string userId);
        bool ArticleExists(int id);
    }

    public class ArticleService : IArticleService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<ArticleService> _logger;

        public ArticleService(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            ILogger<ArticleService> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<List<Article>> GetAllArticlesAsync()
        {
            var articles = await _context.Articles
                .Include(a => a.Author)
                .Include(a => a.Votes)
                .ToListAsync();

            return articles
                .OrderByDescending(a => a.VoteScore)
                .ThenByDescending(a => a.PublishedDate)
                .ToList();
        }

        public async Task<Article?> GetArticleByIdAsync(int? id)
        {
            if (id == null)
            {
                return null;
            }

            return await _context.Articles
                .Include(a => a.Author)
                .Include(a => a.Votes)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<Article?> CreateArticleAsync(Article article, string userId)
        {
            try
            {
                _logger.LogInformation("Creating article: Title={Title}", article.Title);

                article.AuthorId = userId;
                article.PublishedDate = DateTime.Now;

                _context.Add(article);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Article created successfully with ID: {ArticleId}", article.Id);

                return article;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating article");
                return null;
            }
        }

        public async Task<Article?> UpdateArticleAsync(Article article, string userId, bool isAdmin)
        {
            try
            {
                var existingArticle = await _context.Articles.AsNoTracking()
                    .FirstOrDefaultAsync(a => a.Id == article.Id);

                if (existingArticle == null)
                {
                    _logger.LogWarning("Article with ID {Id} not found", article.Id);
                    return null;
                }

                if (!isAdmin && existingArticle.AuthorId != userId)
                {
                    _logger.LogWarning("User tried to edit an article they don't own");
                    return null;
                }

                article.AuthorId = existingArticle.AuthorId;
                article.PublishedDate = existingArticle.PublishedDate;
                article.LastUpdated = DateTime.Now;

                _context.Update(article);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Article updated successfully");

                return article;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating article {Id}", article.Id);
                return null;
            }
        }

        public async Task<bool> DeleteArticleAsync(int id)
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
                _logger.LogError(ex, "Error deleting article {Id}", id);
                return false;
            }
        }

        public async Task<bool> VoteAsync(int articleId, string userId, bool isUpvote)
        {
            try
            {
                var article = await _context.Articles
                    .Include(a => a.Votes)
                    .FirstOrDefaultAsync(a => a.Id == articleId);

                if (article == null)
                {
                    return false;
                }

                var existingVote = article.Votes.FirstOrDefault(v => v.UserId == userId);
                if (existingVote != null)
                {
                    if (existingVote.IsUpvote == isUpvote)
                    {
                        return true;
                    }

                    existingVote.IsUpvote = isUpvote;
                    _context.Update(existingVote);
                }
                else
                {
                    var vote = new Vote
                    {
                        ArticleId = articleId,
                        UserId = userId,
                        IsUpvote = isUpvote
                    };

                    _context.Add(vote);
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error voting on article {Id}", articleId);
                return false;
            }
        }

        public async Task<bool> RemoveVoteAsync(int articleId, string userId)
        {
            try
            {
                var vote = await _context.Votes
                    .FirstOrDefaultAsync(v => v.ArticleId == articleId && v.UserId == userId);

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
                _logger.LogError(ex, "Error removing vote from article {Id}", articleId);
                return false;
            }
        }

        public async Task<Vote?> GetUserVoteAsync(int articleId, string userId)
        {
            return await _context.Votes
                .FirstOrDefaultAsync(v => v.ArticleId == articleId && v.UserId == userId);
        }

        public bool ArticleExists(int id)
        {
            return _context.Articles.Any(e => e.Id == id);
        }
    }
}