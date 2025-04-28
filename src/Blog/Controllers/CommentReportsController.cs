using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Blog.Models;
using Blog.Services;

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
                return BadRequest("Report reason cannot be empty");
            }

            var userId = _userManager.GetUserId(User);

            // Check if user has already reported this comment
            bool hasReported = await _reportService.HasUserReportedCommentAsync(commentId, userId);
            if (hasReported)
            {
                return BadRequest("You have already reported this comment");
            }

            var result = await _reportService.CreateReportAsync(commentId, reason, reportDetails, userId);
            if (result == null)
            {
                return StatusCode(500, "Failed to report comment");
            }

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = true, message = "Comment reported successfully" });
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
                return NotFound("Report not found");
            }

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = true, message = "Report reviewed successfully" });
            }

            return RedirectToAction("Details", new { id = result.Id });
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> BlockComment(int commentId, string reason)
        {
            if (string.IsNullOrWhiteSpace(reason))
            {
                return BadRequest("Block reason cannot be empty");
            }

            var userId = _userManager.GetUserId(User);
            var result = await _reportService.BlockCommentAsync(commentId, reason, userId);

            if (result == null)
            {
                return NotFound("Comment not found");
            }

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = true, message = "Comment blocked successfully" });
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
                return NotFound("Comment not found");
            }

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = true, message = "Comment unblocked successfully" });
            }

            return RedirectToAction("Details", "Articles", new { id = result.ArticleId });
        }
    }
}