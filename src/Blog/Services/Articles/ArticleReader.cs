using Blog.Models;
using Blog.Repositories;
using Blog.Services.Articles.Interfaces;

namespace Blog.Services.Articles
{
    /// <summary>
    /// Implementation of article reading operations
    /// </summary>
    public class ArticleReader : IArticleReader
    {
        private readonly IArticleRepository _repository;
        private readonly ILogger<ArticleReader> _logger;

        public ArticleReader(
            IArticleRepository repository,
            ILogger<ArticleReader> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<List<Article>> GetAllArticlesAsync()
        {
            _logger.LogInformation("Getting all articles");
            var articles = await _repository.GetAllAsync();

            return articles
                .OrderByDescending(a => a.VoteScore)
                .ThenByDescending(a => a.PublishedDate)
                .ToList();
        }

        public async Task<Article?> GetArticleByIdAsync(int? id)
        {
            if (id == null)
            {
                _logger.LogWarning("Attempted to get article with null ID");
                return null;
            }

            _logger.LogInformation("Getting article with ID: {ArticleId}", id.Value);
            return await _repository.GetByIdAsync(id.Value);
        }

        public bool ArticleExists(int id)
        {
            return _repository.ExistsAsync(id).GetAwaiter().GetResult();
        }
    }
}