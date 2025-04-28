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
    }
}