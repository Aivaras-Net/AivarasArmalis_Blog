using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Blog.Models;
using Blog.Services;

namespace Blog.Controllers
{
    [Authorize]
    public class CommentsController : Controller
    {
        private readonly ICommentService _commentService;
        private readonly IArticleService _articleService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<CommentsController> _logger;

        public CommentsController(
            ICommentService commentService,
            IArticleService articleService,
            UserManager<ApplicationUser> userManager,
            ILogger<CommentsController> logger)
        {
            _commentService = commentService;
            _articleService = articleService;
            _userManager = userManager;
            _logger = logger;
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Commentator")]
        public async Task<IActionResult> Create(int articleId, string content, int? parentCommentId)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return BadRequest("Comment content cannot be empty");
            }

            var article = await _articleService.GetArticleByIdAsync(articleId);
            if (article == null)
            {
                return NotFound("Article not found");
            }

            if (parentCommentId.HasValue)
            {
                var parentComment = await _commentService.GetCommentByIdAsync(parentCommentId.Value);
                if (parentComment == null)
                {
                    return NotFound("Parent comment not found");
                }
            }

            var userId = _userManager.GetUserId(User);
            var comment = new Comment
            {
                ArticleId = articleId,
                Content = content,
                ParentCommentId = parentCommentId
            };

            var createdComment = await _commentService.CreateCommentAsync(comment, userId);
            if (createdComment == null)
            {
                return StatusCode(500, "Failed to create comment");
            }

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_CommentPartial", createdComment);
            }

            return RedirectToAction("Details", "Articles", new { id = articleId });
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Writer,Commentator")]
        public async Task<IActionResult> Edit(int id, string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return BadRequest("Comment content cannot be empty");
            }

            var userId = _userManager.GetUserId(User);
            var isAdmin = User.IsInRole("Admin");

            var updatedComment = await _commentService.UpdateCommentAsync(id, content, userId, isAdmin);
            if (updatedComment == null)
            {
                return NotFound("Comment not found or you don't have permission to edit it");
            }

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_CommentPartial", updatedComment);
            }

            return RedirectToAction("Details", "Articles", new { id = updatedComment.ArticleId });
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Writer,Commentator")]
        public async Task<IActionResult> Delete(int id)
        {
            var comment = await _commentService.GetCommentByIdAsync(id);
            if (comment == null)
            {
                return NotFound("Comment not found");
            }

            var userId = _userManager.GetUserId(User);
            var isAdmin = User.IsInRole("Admin");

            var result = await _commentService.DeleteCommentAsync(id, userId, isAdmin);
            if (!result)
            {
                return StatusCode(500, "Failed to delete comment");
            }

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = true });
            }

            return RedirectToAction("Details", "Articles", new { id = comment.ArticleId });
        }

        [HttpGet]
        public async Task<IActionResult> GetReplies(int commentId)
        {
            var replies = await _commentService.GetRepliesAsync(commentId);

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_RepliesPartial", replies);
            }

            return Json(replies);
        }
    }
}