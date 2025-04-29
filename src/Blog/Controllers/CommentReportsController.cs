using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Blog.Models;
using Blog.Services.Comments;
using Blog.Services.Comments.Interfaces;

namespace Blog.Controllers
{
    [Authorize]
    public class CommentReportsController : Controller
    {
        private readonly ICommentReportService _reportService;
        private readonly ICommentService _commentService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<CommentReportsController> _logger;

        public CommentReportsController(
            ICommentReportService reportService,
            ICommentService commentService,
            UserManager<ApplicationUser> userManager,
            ILogger<CommentReportsController> logger)
        {
            _reportService = reportService;
            _commentService = commentService;
            _userManager = userManager;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> Report(int commentId, string reason, string reportDetails)
        {
            if (string.IsNullOrWhiteSpace(reason))
            {
                return BadRequest(WebConstants.CommentReportReasonEmpty);
            }

            var userId = _userManager.GetUserId(User);

            bool hasReported = await _reportService.HasUserReportedCommentAsync(commentId, userId);
            if (hasReported)
            {
                return BadRequest(WebConstants.CommentAlreadyReported);
            }

            var result = await _reportService.CreateReportAsync(commentId, reason, reportDetails, userId);
            if (result == null)
            {
                return StatusCode(500, WebConstants.CommentReportError);
            }

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = true, message = WebConstants.CommentReportSuccess });
            }

            return RedirectToAction("Details", "Articles", new { id = result.Comment.ArticleId });
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            var reports = await _reportService.GetAllReportsAsync();
            return View(reports);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Pending()
        {
            var reports = await _reportService.GetPendingReportsAsync();
            return View(reports);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Details(int id)
        {
            var report = await _reportService.GetReportByIdAsync(id);
            if (report == null)
            {
                return NotFound();
            }

            return View(report);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Review(int id, ReportStatus status, string notes)
        {
            var userId = _userManager.GetUserId(User);
            var result = await _reportService.UpdateReportStatusAsync(id, status, notes, userId);

            if (result == null)
            {
                return NotFound(WebConstants.NotFound);
            }

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = true, message = WebConstants.CommentReportReviewed });
            }

            return RedirectToAction("Details", new { id = result.Id });
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> BlockComment(int commentId, string reason)
        {
            if (string.IsNullOrWhiteSpace(reason))
            {
                return BadRequest(WebConstants.CommentReportReasonEmpty);
            }

            var userId = _userManager.GetUserId(User);
            var result = await _reportService.BlockCommentAsync(commentId, reason, userId);

            if (result == null)
            {
                return NotFound(WebConstants.CommentNotFound);
            }

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = true, message = WebConstants.CommentBlockSuccess });
            }

            return RedirectToAction("Details", "Articles", new { id = result.ArticleId });
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UnblockComment(int commentId)
        {
            var userId = _userManager.GetUserId(User);
            var result = await _reportService.UnblockCommentAsync(commentId, userId);

            if (result == null)
            {
                return NotFound(WebConstants.CommentNotFound);
            }

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = true, message = WebConstants.CommentUnblocked });
            }

            return RedirectToAction("Details", "Articles", new { id = result.ArticleId });
        }
    }
}