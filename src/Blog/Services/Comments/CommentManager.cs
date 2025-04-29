using Blog.Models;
using Blog.Repositories;
using Blog.Services.Comments.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace Blog.Services.Comments
{
    /// <summary>
    /// Service for managing comments
    /// </summary>
    public class CommentManager : ICommentManager
    {
        private readonly ICommentRepository _commentRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<CommentManager> _logger;

        public CommentManager(
            ICommentRepository commentRepository,
            UserManager<ApplicationUser> userManager,
            ILogger<CommentManager> logger)
        {
            _commentRepository = commentRepository;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<Comment?> CreateCommentAsync(Comment comment, string userId)
        {
            try
            {
                _logger.LogInformation("Creating comment for article {ArticleId}", comment.ArticleId);

                comment.AuthorId = userId;
                comment.CreatedAt = DateTime.Now;

                await _commentRepository.AddAsync(comment);
                var createdComment = await _commentRepository.GetByIdAsync(comment.Id);

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
                var comment = await _commentRepository.GetByIdAsync(id, false);

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

                await _commentRepository.UpdateAsync(comment);
                var updatedComment = await _commentRepository.GetByIdAsync(comment.Id);

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
                var comment = await _commentRepository.GetByIdAsync(id, false);

                if (comment == null)
                {
                    return false;
                }

                if (!isAdmin && comment.AuthorId != userId)
                {
                    _logger.LogWarning("Unauthorized delete attempt on comment {CommentId}", id);
                    return false;
                }

                var result = await _commentRepository.DeleteAsync(id);
                if (result)
                {
                    _logger.LogInformation("Comment {CommentId} deleted successfully", id);
                }
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting comment {CommentId}", id);
                return false;
            }
        }
    }
}