using Blog.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Blog.Services;
using Blog.Services.Articles;

namespace Blog.Controllers
{
    public class ArticlesController : Controller
    {
        private readonly IArticleReader _articleReader;
        private readonly IArticleWriter _articleWriter;
        private readonly IArticleVoting _articleVoting;
        private readonly IArticleMappingService _mapper;
        private readonly IValidationService _validationService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<ArticlesController> _logger;

        public ArticlesController(
            IArticleReader articleReader,
            IArticleWriter articleWriter,
            IArticleVoting articleVoting,
            IArticleMappingService mapper,
            IValidationService validationService,
            UserManager<ApplicationUser> userManager,
            ILogger<ArticlesController> logger)
        {
            _articleReader = articleReader;
            _articleWriter = articleWriter;
            _articleVoting = articleVoting;
            _mapper = mapper;
            _validationService = validationService;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var articles = await _articleReader.GetAllArticlesAsync();
            return View(articles);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var article = await _articleReader.GetArticleByIdAsync(id);

            if (article == null)
            {
                return NotFound();
            }

            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var userVote = await _articleVoting.GetUserVoteAsync(article.Id, userId);
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
            _logger.LogInformation(WebConstants.LogArticleCreateCalled,
                article.Title, article.Summary?.Length ?? 0, article.Content?.Length ?? 0);

            if (!_validationService.ValidateArticle(article, ModelState))
            {
                LogValidationErrors();
                return View(article);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _logger.LogInformation(WebConstants.LogSetAuthorId, userId);

            var createdArticle = await _articleWriter.CreateArticleAsync(article, userId ?? string.Empty);

            if (createdArticle != null)
            {
                _logger.LogInformation(WebConstants.LogArticleCreated, createdArticle.Id);
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError("", WebConstants.ArticleCreationError);
            return View(article);
        }

        [Authorize(Roles = "Admin,Writer")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var article = await _articleReader.GetArticleByIdAsync(id);
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
            _logger.LogInformation(WebConstants.LogArticleEditCalled,
                id, article.Summary?.Length ?? 0, article.Content?.Length ?? 0);

            if (id != article.Id)
            {
                _logger.LogWarning(WebConstants.LogIdMismatch, id, article.Id);
                return NotFound();
            }

            if (!_validationService.ValidateArticle(article, ModelState))
            {
                LogValidationErrors();
                return View(article);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            bool isAdmin = User.IsInRole("Admin");

            var updatedArticle = await _articleWriter.UpdateArticleAsync(article, userId ?? string.Empty, isAdmin);

            if (updatedArticle != null)
            {
                _logger.LogInformation(WebConstants.LogArticleUpdated);
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError("", WebConstants.ArticleUpdateError);
            return View(article);
        }

        [Authorize(Roles = "Admin,Writer")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var article = await _articleReader.GetArticleByIdAsync(id);

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
            var article = await _articleReader.GetArticleByIdAsync(id);
            if (article == null)
            {
                return NotFound();
            }

            if (!User.IsInRole("Admin") && article.AuthorId != User.FindFirstValue(ClaimTypes.NameIdentifier))
            {
                return Forbid();
            }

            var result = await _articleWriter.DeleteArticleAsync(id);
            if (result)
            {
                _logger.LogInformation(WebConstants.ArticleDeleteSuccess);
                return RedirectToAction(nameof(Index));
            }

            _logger.LogError(WebConstants.ArticleDeleteError, id);
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

            var result = await _articleVoting.VoteAsync(id, userId, isUpvote);
            if (!result)
            {
                _logger.LogWarning(WebConstants.LogVoteFailed, id);
                return BadRequest(WebConstants.ArticleVoteError);
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

            var result = await _articleVoting.RemoveVoteAsync(id, userId);
            if (!result)
            {
                _logger.LogWarning(WebConstants.LogRemoveVoteFailed, id);
                return BadRequest(WebConstants.ArticleRemoveVoteError);
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        private void LogValidationErrors()
        {
            foreach (var state in ModelState)
            {
                if (state.Value.Errors.Any())
                {
                    _logger.LogWarning(WebConstants.LogValidationError,
                        state.Key, string.Join(", ", state.Value.Errors.Select(e => e.ErrorMessage)));
                }
            }
        }
    }
}