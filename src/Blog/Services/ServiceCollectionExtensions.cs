using Blog.Repositories;
using Blog.Services.Articles;
using Blog.Services.Articles.Interfaces;
using Blog.Services.Comments;
using Blog.Services.Comments.Interfaces;

namespace Blog.Services
{
    /// <summary>
    /// Extension methods for registering application services
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Registers all article-related services
        /// </summary>
        public static IServiceCollection AddArticleServices(this IServiceCollection services)
        {
            services.AddScoped<IArticleRepository, ArticleRepository>();

            services.AddScoped<IArticleMappingService, ArticleMappingService>();

            services.AddScoped<IArticleReader, ArticleReader>();
            services.AddScoped<IArticleWriter, ArticleWriter>();
            services.AddScoped<IArticleVoting, ArticleVoting>();

            return services;
        }

        /// <summary>
        /// Registers all validation-related services
        /// </summary>
        public static IServiceCollection AddValidationServices(this IServiceCollection services)
        {
            services.AddScoped<IValidationService, ValidationService>();
            return services;
        }

        /// <summary>
        /// Registers all comment-related services
        /// </summary>
        public static IServiceCollection AddCommentServices(this IServiceCollection services)
        {
            services.AddScoped<ICommentRepository, CommentRepository>();

            services.AddScoped<ICommentReader, CommentReader>();
            services.AddScoped<ICommentManager, CommentManager>();

            services.AddScoped<ICommentService, CommentService>();

            return services;
        }
    }
}