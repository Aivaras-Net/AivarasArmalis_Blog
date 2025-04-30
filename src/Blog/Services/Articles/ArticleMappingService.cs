using Blog.Models;
using Blog.Models.Dtos;

namespace Blog.Services.Articles
{
    /// <summary>
    /// Interface for mapping between article entities and DTOs
    /// </summary>
    public interface IArticleMappingService
    {
        ArticleListItemDto MapToListItemDto(Article article);
        List<ArticleListItemDto> MapToListItemDto(IEnumerable<Article> articles);
        ArticleDetailDto MapToDetailDto(Article article, Vote? userVote = null);
        Article MapCreateDtoToEntity(ArticleCreateDto dto);
        Article MapUpdateDtoToEntity(ArticleUpdateDto dto, Article existingArticle);
    }

    /// <summary>
    /// Implementation of mapping service between article entities and DTOs
    /// </summary>
    public class ArticleMappingService : IArticleMappingService
    {
        public ArticleListItemDto MapToListItemDto(Article article)
        {
            return new ArticleListItemDto
            {
                Id = article.Id,
                Title = article.Title,
                Summary = article.Summary,
                Content = article.Content,
                ImageUrl = article.ImageUrl,
                PublishedDate = article.PublishedDate,
                LastUpdated = article.LastUpdated,
                Author = new AuthorDto
                {
                    Name = article.Author != null ? $"{article.Author.FirstName} {article.Author.LastName}" : string.Empty
                },
                Votes = new VoteStatsDto
                {
                    Score = article.VoteScore,
                    UpvoteCount = article.UpvoteCount,
                    DownvoteCount = article.DownvoteCount
                }
            };
        }

        public List<ArticleListItemDto> MapToListItemDto(IEnumerable<Article> articles)
        {
            return articles.Select(MapToListItemDto).ToList();
        }

        public ArticleDetailDto MapToDetailDto(Article article, Vote? userVote = null)
        {
            var dto = new ArticleDetailDto
            {
                Id = article.Id,
                Title = article.Title,
                Summary = article.Summary,
                Content = article.Content,
                ImageUrl = article.ImageUrl,
                PublishedDate = article.PublishedDate,
                LastUpdated = article.LastUpdated,
                Author = new AuthorDto
                {
                    Name = article.Author != null ? $"{article.Author.FirstName} {article.Author.LastName}" : string.Empty
                },
                Votes = new VoteStatsDto
                {
                    Score = article.VoteScore,
                    UpvoteCount = article.UpvoteCount,
                    DownvoteCount = article.DownvoteCount,
                    CurrentUserVoted = userVote != null,
                    CurrentUserVotedUp = userVote?.IsUpvote
                }
            };

            if (article.Comments != null && article.Comments.Any())
            {
                var parentComments = article.Comments
                    .Where(c => c.ParentCommentId == null && !c.IsBlocked)
                    .OrderByDescending(c => c.CreatedAt);

                foreach (var comment in parentComments)
                {
                    dto.Comments.Add(MapCommentToDto(comment, article.Comments));
                }
            }

            return dto;
        }

        public Article MapCreateDtoToEntity(ArticleCreateDto dto)
        {
            return new Article
            {
                Title = dto.Title,
                Summary = dto.Summary,
                Content = dto.Content,
                ImageUrl = dto.ImageUrl
            };
        }

        public Article MapUpdateDtoToEntity(ArticleUpdateDto dto, Article existingArticle)
        {
            existingArticle.Title = dto.Title;
            existingArticle.Summary = dto.Summary;
            existingArticle.Content = dto.Content;
            existingArticle.ImageUrl = dto.ImageUrl;

            return existingArticle;
        }

        private CommentDto MapCommentToDto(Comment comment, IEnumerable<Comment> allComments)
        {
            var dto = new CommentDto
            {
                Id = comment.Id,
                Content = comment.Content,
                CreatedAt = comment.CreatedAt,
                Author = new AuthorDto
                {
                    Name = comment.Author != null ? $"{comment.Author.FirstName} {comment.Author.LastName}" : string.Empty
                },
                ParentCommentId = comment.ParentCommentId
            };

            var replies = allComments
                .Where(c => c.ParentCommentId == comment.Id && !c.IsBlocked)
                .OrderBy(c => c.CreatedAt);

            foreach (var reply in replies)
            {
                dto.Replies.Add(MapCommentToDto(reply, allComments));
            }

            return dto;
        }
    }
}