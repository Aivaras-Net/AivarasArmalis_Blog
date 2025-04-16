using Blog.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Blog.Services;

namespace Blog.Controllers
{
    public class ArticlesController : Controller
    {
        private readonly IArticleService _articleService;
        private readonly IValidationService _validationService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<ArticlesController> _logger;

        public ArticlesController(
            IArticleService articleService,
            IValidationService validationService,
            UserManager<ApplicationUser> userManager,
            ILogger<ArticlesController> logger)
        {
            _articleService = articleService;
            _validationService = validationService;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var articles = await _articleService.GetAllArticlesAsync();
            return View(articles);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var article = await _articleService.GetArticleByIdAsync(id);

            if (article == null)
            {
                return NotFound();
            }

            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var userVote = await _articleService.GetUserVoteAsync(article.Id, userId);
                ViewBag.CurrentUserVote = userVote;
            }

            return View(article);
        }

        [Authorize(Roles = "Admin,Writer")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Writer")]
        public async Task<IActionResult> Create(Article article)
        {
            _logger.LogInformation("Create POST called with article: Title={Title}, SummaryLength={SummaryLength}, ContentLength={ContentLength}",
                article.Title, article.Summary?.Length ?? 0, article.Content?.Length ?? 0);

            if (!_validationService.ValidateArticle(article, ModelState))
            {
                LogValidationErrors();
                return View(article);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _logger.LogInformation("Setting AuthorId to current user: {UserId}", userId);

            var createdArticle = await _articleService.CreateArticleAsync(article, userId ?? string.Empty);

            if (createdArticle != null)
            {
                _logger.LogInformation("Article created successfully with ID: {ArticleId}", createdArticle.Id);
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError("", "An error occurred while saving the article. Please try again.");
            return View(article);
        }

        [Authorize(Roles = "Admin,Writer")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var article = await _articleService.GetArticleByIdAsync(id);
            if (article == null)
            {
                return NotFound();
            }

            if (!User.IsInRole("Admin") && article.AuthorId != User.FindFirstValue(ClaimTypes.NameIdentifier))
            {
                return Forbid();
            }

            return View(article);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Writer")]
        public async Task<IActionResult> Edit(int id, Article article)
        {
            _logger.LogInformation("Edit POST called with article ID: {Id}, SummaryLength: {SummaryLength}, ContentLength: {ContentLength}",
                id, article.Summary?.Length ?? 0, article.Content?.Length ?? 0);

            if (id != article.Id)
            {
                _logger.LogWarning("ID mismatch: {RouteId} vs {ModelId}", id, article.Id);
                return NotFound();
            }

            if (!_validationService.ValidateArticle(article, ModelState))
            {
                LogValidationErrors();
                return View(article);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            bool isAdmin = User.IsInRole("Admin");

            var updatedArticle = await _articleService.UpdateArticleAsync(article, userId ?? string.Empty, isAdmin);

            if (updatedArticle != null)
            {
                _logger.LogInformation("Article updated successfully");
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError("", "An error occurred while updating the article. Please try again.");
            return View(article);
        }

        [Authorize(Roles = "Admin,Writer")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var article = await _articleService.GetArticleByIdAsync(id);

            if (article == null)
            {
                return NotFound();
            }

            if (!User.IsInRole("Admin") && article.AuthorId != User.FindFirstValue(ClaimTypes.NameIdentifier))
            {
                return Forbid();
            }

            return View(article);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Writer")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var article = await _articleService.GetArticleByIdAsync(id);
            if (article == null)
            {
                return NotFound();
            }

            if (!User.IsInRole("Admin") && article.AuthorId != User.FindFirstValue(ClaimTypes.NameIdentifier))
            {
                return Forbid();
            }

            var result = await _articleService.DeleteArticleAsync(id);
            if (result)
            {
                _logger.LogInformation("Article deleted successfully");
                return RedirectToAction(nameof(Index));
            }

            _logger.LogError("Failed to delete article with ID {Id}", id);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Critic")]
        public async Task<IActionResult> Vote(int id, bool isUpvote)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var result = await _articleService.VoteAsync(id, userId, isUpvote);
            if (!result)
            {
                _logger.LogWarning("Vote failed for article {Id}", id);
                return BadRequest("Failed to vote on the article");
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Critic")]
        public async Task<IActionResult> RemoveVote(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var result = await _articleService.RemoveVoteAsync(id, userId);
            if (!result)
            {
                _logger.LogWarning("Remove vote failed for article {Id}", id);
                return BadRequest("Failed to remove vote from the article");
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        private void LogValidationErrors()
        {
            foreach (var state in ModelState)
            {
                if (state.Value.Errors.Any())
                {
                    _logger.LogWarning("Validation error for {Property}: {Error}",
                        state.Key, string.Join(", ", state.Value.Errors.Select(e => e.ErrorMessage)));
                }
            }
        }
    }
}