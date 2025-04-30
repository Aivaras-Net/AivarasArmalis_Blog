using Blog.Models;
using Blog.Repositories;
using Blog.Services.Articles.Interfaces;


namespace Blog.Services.Articles
{
    /// <summary>
    /// Implementation of article writing operations
    /// </summary>
    public class ArticleWriter : IArticleWriter
    {
        private readonly IArticleRepository _repository;
        private readonly ILogger<ArticleWriter> _logger;

        public ArticleWriter(
            IArticleRepository repository,
            ILogger<ArticleWriter> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<Article?> CreateArticleAsync(Article article, string userId)
        {
            try
            {
                _logger.LogInformation(WebConstants.LogCreatingArticle, article.Title);

                article.AuthorId = userId;
                article.PublishedDate = DateTime.Now;

                await _repository.AddAsync(article);
                _logger.LogInformation(WebConstants.LogArticleCreated, article.Id);

                return article;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, WebConstants.LogErrorCreatingArticle);
                return null;
            }
        }

        public async Task<Article?> UpdateArticleAsync(Article article, string userId, bool isAdmin)
        {
            try
            {
                var existingArticle = await _repository.GetByIdAsync(article.Id, includeVotes: false);

                if (existingArticle == null)
                {
                    _logger.LogWarning(WebConstants.LogArticleNotFound, article.Id);
                    return null;
                }

                if (!isAdmin && existingArticle.AuthorId != userId)
                {
                    _logger.LogWarning(WebConstants.LogUnauthorizedEdit);
                    return null;
                }

                existingArticle.Title = article.Title;
                existingArticle.Summary = article.Summary;
                existingArticle.Content = article.Content;
                existingArticle.ImageUrl = article.ImageUrl;
                existingArticle.LastUpdated = DateTime.Now;

                var updatedArticle = await _repository.UpdateAsync(existingArticle);
                _logger.LogInformation(WebConstants.LogArticleUpdated);

                return updatedArticle;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, WebConstants.LogErrorUpdatingArticle, article.Id);
                return null;
            }
        }

        public async Task<bool> DeleteArticleAsync(int id)
        {
            _logger.LogInformation("Deleting article with ID: {ArticleId}", id);
            return await _repository.DeleteAsync(id);
        }
    }
}