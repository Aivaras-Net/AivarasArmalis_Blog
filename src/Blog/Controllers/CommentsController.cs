using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Blog.Models;
using Blog.Services.Comments.Interfaces;
using Blog.Services.Articles.Interfaces;

namespace Blog.Controllers
{
    [Authorize]
    public class CommentsController : Controller
    {
        private readonly ICommentService _commentService;
        private readonly IArticleReader _articleReader;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<CommentsController> _logger;

        public CommentsController(
            ICommentService commentService,
            IArticleReader articleReader,
            UserManager<ApplicationUser> userManager,
            ILogger<CommentsController> logger)
        {
            _commentService = commentService;
            _articleReader = articleReader;
            _userManager = userManager;
            _logger = logger;
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Commentator")]
        public async Task<IActionResult> Create(int articleId, string content, int? parentCommentId)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return BadRequest(WebConstants.CommentContentEmpty);
            }

            var article = await _articleReader.GetArticleByIdAsync(articleId);
            if (article == null)
            {
                return NotFound(WebConstants.NotFound);
            }

            int? rootCommentId = null;
            if (parentCommentId.HasValue)
            {
                var parentComment = await _commentService.GetCommentByIdAsync(parentCommentId.Value);
                if (parentComment == null)
                {
                    return NotFound(WebConstants.CommentNotFound);
                }

                if (parentComment.ParentCommentId.HasValue)
                {
                    rootCommentId = parentComment.ParentCommentId.Value;
                }
                else
                {
                    rootCommentId = parentComment.Id;
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
                return StatusCode(500, WebConstants.CommentCreationError);
            }

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                if (parentCommentId.HasValue)
                {
                    if (rootCommentId.HasValue)
                    {
                        return PartialView("_CommentPartial", createdComment);
                    }
                    return PartialView("_CommentPartial", createdComment);
                }
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
                return BadRequest(WebConstants.CommentContentEmpty);
            }

            var comment = await _commentService.GetCommentByIdAsync(id);
            if (comment == null)
            {
                return NotFound(WebConstants.CommentNotFound);
            }

            if (comment.IsBlocked && !User.IsInRole("Admin"))
            {
                return BadRequest(WebConstants.CommentBlocked);
            }

            var userId = _userManager.GetUserId(User);
            var isAdmin = User.IsInRole("Admin");

            var updatedComment = await _commentService.UpdateCommentAsync(id, content, userId, isAdmin);
            if (updatedComment == null)
            {
                return NotFound(WebConstants.CommentUpdateError);
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
                return NotFound(WebConstants.CommentNotFound);
            }

            if (comment.IsBlocked && !User.IsInRole("Admin"))
            {
                return BadRequest(WebConstants.CommentBlockedAdminOnly);
            }

            var userId = _userManager.GetUserId(User);
            var isAdmin = User.IsInRole("Admin");

            var result = await _commentService.DeleteCommentAsync(id, userId, isAdmin);
            if (!result)
            {
                return StatusCode(500, WebConstants.CommentDeleteError);
            }

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = true, commentId = id });
            }

            return RedirectToAction("Details", "Articles", new { id = comment.ArticleId });
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetReplies(int commentId)
        {
            bool includeBlocked = User.IsInRole("Admin");
            var replies = await _commentService.GetRepliesAsync(commentId, includeBlocked);

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_RepliesPartial", replies);
            }

            return Json(replies);
        }
    }
}