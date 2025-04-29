using Blog.Services.Comments.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Blog.ViewComponents
{
    public class ArticleCommentsViewComponent : ViewComponent
    {
        private readonly ICommentService _commentService;

        public ArticleCommentsViewComponent(ICommentService commentService)
        {
            _commentService = commentService;
        }

        public async Task<IViewComponentResult> InvokeAsync(int articleId)
        {
            var comments = await _commentService.GetArticleCommentsAsync(articleId);
            return View(comments);
        }
    }
}