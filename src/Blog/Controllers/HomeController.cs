using Blog.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Blog.Services;
using Microsoft.EntityFrameworkCore;

namespace Blog.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IArticleService _articleService;
        private readonly ApplicationDbContext _context;

        public HomeController(
            ILogger<HomeController> logger,
            IArticleService articleService,
            ApplicationDbContext context)
        {
            _logger = logger;
            _articleService = articleService;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var homeViewModel = new HomeViewModel
            {
                RecentArticles = await _context.Articles
                    .Include(a => a.Author)
                    .Include(a => a.Votes)
                    .OrderByDescending(a => a.PublishedDate)
                    .Take(5)
                    .ToListAsync(),

                TopRankedArticles = await _context.Articles
                    .Include(a => a.Author)
                    .Include(a => a.Votes)
                    .OrderByDescending(a => a.Votes.Sum(v => v.IsUpvote ? 1 : -1))
                    .ThenByDescending(a => a.PublishedDate)
                    .Take(3)
                    .ToListAsync(),

                RecentlyCommentedArticles = await _context.Articles
                    .Include(a => a.Author)
                    .Include(a => a.Votes)
                    .Include(a => a.Comments)
                    .Where(a => a.Comments.Any())
                    .OrderByDescending(a => a.Comments.Max(c => c.CreatedAt))
                    .Take(3)
                    .ToListAsync()
            };

            return View(homeViewModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
