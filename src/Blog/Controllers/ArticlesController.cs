using Blog.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Microsoft.Extensions.Logging;

namespace Blog.Controllers
{
    public class ArticlesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<ArticlesController> _logger;

        public ArticlesController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            ILogger<ArticlesController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var articles = await _context.Articles
                .Include(a => a.Author)
                .Include(a => a.Votes)
                .ToListAsync();

            articles = articles
                .OrderByDescending(a => a.VoteScore)
                .ThenByDescending(a => a.PublishedDate)
                .ToList();

            return View(articles);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var article = await _context.Articles
                .Include(a => a.Author)
                .Include(a => a.Votes)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (article == null)
            {
                return NotFound();
            }

            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                ViewBag.CurrentUserVote = article.Votes
                    .FirstOrDefault(v => v.UserId == userId);
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

            ModelState.Remove("AuthorId");

            if (string.IsNullOrWhiteSpace(article.Title))
            {
                ModelState.AddModelError("Title", "Title is required");
            }
            else
            {
                ModelState.Remove("Summary");
                ModelState.Remove("Content");
                ModelState.Remove("ImageUrl");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    _logger.LogInformation("Setting AuthorId to current user: {UserId}", userId);
                    article.AuthorId = userId ?? string.Empty;
                    article.PublishedDate = DateTime.Now;

                    _context.Add(article);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Article created successfully with ID: {ArticleId}", article.Id);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating article");
                    ModelState.AddModelError("", "An error occurred while saving the article. Please try again.");
                    return View(article);
                }
            }

            foreach (var state in ModelState)
            {
                if (state.Value.Errors.Any())
                {
                    _logger.LogWarning("Validation error for {Property}: {Error}",
                        state.Key, string.Join(", ", state.Value.Errors.Select(e => e.ErrorMessage)));
                }
            }

            return View(article);
        }

        [Authorize(Roles = "Admin,Writer")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var article = await _context.Articles.FindAsync(id);
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

            var existingArticle = await _context.Articles.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id);
            if (existingArticle == null)
            {
                _logger.LogWarning("Article with ID {Id} not found", id);
                return NotFound();
            }

            if (!User.IsInRole("Admin") && existingArticle.AuthorId != User.FindFirstValue(ClaimTypes.NameIdentifier))
            {
                _logger.LogWarning("User tried to edit an article they don't own");
                return Forbid();
            }

            ModelState.Remove("AuthorId");

            if (string.IsNullOrWhiteSpace(article.Title))
            {
                ModelState.AddModelError("Title", "Title is required");
            }
            else
            {
                ModelState.Remove("Summary");
                ModelState.Remove("Content");
                ModelState.Remove("ImageUrl");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    article.AuthorId = existingArticle.AuthorId;
                    article.PublishedDate = existingArticle.PublishedDate;
                    article.LastUpdated = DateTime.Now;

                    _context.Update(article);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Article updated successfully");
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    if (!ArticleExists(article.Id))
                    {
                        _logger.LogWarning("Concurrency error: Article with ID {Id} no longer exists", article.Id);
                        return NotFound();
                    }
                    else
                    {
                        _logger.LogError(ex, "Concurrency error updating article {Id}", article.Id);
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating article {Id}", article.Id);
                    ModelState.AddModelError("", "An error occurred while updating the article. Please try again.");
                    return View(article);
                }
            }

            foreach (var state in ModelState)
            {
                if (state.Value.Errors.Any())
                {
                    _logger.LogWarning("Validation error for {Property}: {Error}",
                        state.Key, string.Join(", ", state.Value.Errors.Select(e => e.ErrorMessage)));
                }
            }

            return View(article);
        }

        [Authorize(Roles = "Admin,Writer")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var article = await _context.Articles
                .Include(a => a.Author)
                .FirstOrDefaultAsync(m => m.Id == id);

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
            var article = await _context.Articles.FindAsync(id);
            if (article == null)
            {
                return NotFound();
            }

            if (!User.IsInRole("Admin") && article.AuthorId != User.FindFirstValue(ClaimTypes.NameIdentifier))
            {
                return Forbid();
            }

            _context.Articles.Remove(article);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ArticleExists(int id)
        {
            return _context.Articles.Any(e => e.Id == id);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Critic")]
        public async Task<IActionResult> Vote(int id, bool isUpvote)
        {
            var article = await _context.Articles
                .Include(a => a.Votes)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (article == null)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return RedirectToAction(nameof(Details), new { id });
            }

            var existingVote = await _context.Votes
                .FirstOrDefaultAsync(v => v.ArticleId == id && v.UserId == userId);

            if (existingVote != null)
            {
                if (existingVote.IsUpvote != isUpvote)
                {
                    existingVote.IsUpvote = isUpvote;
                    existingVote.CreatedDate = DateTime.Now;
                    _context.Update(existingVote);
                    await _context.SaveChangesAsync();
                }
            }
            else
            {
                var vote = new Vote
                {
                    ArticleId = id,
                    UserId = userId ?? string.Empty,
                    IsUpvote = isUpvote,
                    CreatedDate = DateTime.Now
                };

                _context.Votes.Add(vote);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Critic")]
        public async Task<IActionResult> RemoveVote(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return RedirectToAction(nameof(Details), new { id });
            }

            var vote = await _context.Votes
                .FirstOrDefaultAsync(v => v.ArticleId == id && v.UserId == userId);

            if (vote != null)
            {
                _context.Votes.Remove(vote);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Details), new { id });
        }
    }
}