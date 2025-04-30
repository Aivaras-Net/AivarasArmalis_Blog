using Blog.Models.Dtos;
using Blog.Services.Articles;
using Blog.Services.Articles.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Blog.Models;

namespace Blog.Controllers.Api
{
    [Route("api/articles")]
    [ApiController]
    [Produces("application/json")]
    public class ArticlesApiController : ControllerBase
    {
        private readonly IArticleReader _articleReader;
        private readonly IArticleWriter _articleWriter;
        private readonly IArticleVoting _articleVoting;
        private readonly IArticleMappingService _mapper;
        private readonly ILogger<ArticlesApiController> _logger;

        public ArticlesApiController(
            IArticleReader articleReader,
            IArticleWriter articleWriter,
            IArticleVoting articleVoting,
            IArticleMappingService mapper,
            ILogger<ArticlesApiController> logger)
        {
            _articleReader = articleReader;
            _articleWriter = articleWriter;
            _articleVoting = articleVoting;
            _mapper = mapper;
            _logger = logger;
        }

        /// <summary>
        /// Gets all articles
        /// </summary>
        /// <returns>A list of all articles</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ArticleListItemDto>>> GetArticles()
        {
            var articles = await _articleReader.GetAllArticlesAsync();
            var articleDtos = _mapper.MapToListItemDto(articles);
            return Ok(articleDtos);
        }

        /// <summary>
        /// Gets a specific article by id
        /// </summary>
        /// <param name="id">The article id</param>
        /// <returns>The article if found</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ArticleDetailDto>> GetArticle(int id)
        {
            var article = await _articleReader.GetArticleByIdAsync(id);

            if (article == null)
            {
                return NotFound();
            }

            Vote? userVote = null;
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId != null)
                {
                    userVote = await _articleVoting.GetUserVoteAsync(article.Id, userId);
                }
            }

            var articleDto = _mapper.MapToDetailDto(article, userVote);
            return Ok(articleDto);
        }

        /// <summary>
        /// Creates a new article
        /// </summary>
        /// <param name="articleDto">The article to create</param>
        /// <returns>The created article</returns>
        [HttpPost]
        [Authorize(Roles = "Admin,Writer")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ArticleDetailDto>> CreateArticle([FromBody] ArticleCreateDto articleDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var article = _mapper.MapCreateDtoToEntity(articleDto);
            var createdArticle = await _articleWriter.CreateArticleAsync(article, userId);

            if (createdArticle == null)
            {
                return BadRequest("Failed to create article");
            }

            var resultDto = _mapper.MapToDetailDto(createdArticle);
            return CreatedAtAction(nameof(GetArticle), new { id = resultDto.Id }, resultDto);
        }

        /// <summary>
        /// Updates an existing article
        /// </summary>
        /// <param name="id">The article id</param>
        /// <param name="articleDto">The updated article data</param>
        /// <returns>No content if successful</returns>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Writer")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateArticle(int id, [FromBody] ArticleUpdateDto articleDto)
        {
            articleDto.Id = id;

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var existingArticle = await _articleReader.GetArticleByIdAsync(id);
            if (existingArticle == null)
            {
                return NotFound();
            }

            bool isAdmin = User.IsInRole("Admin");
            if (!isAdmin && existingArticle.AuthorId != userId)
            {
                return Forbid();
            }

            var article = _mapper.MapUpdateDtoToEntity(articleDto, existingArticle);
            var updatedArticle = await _articleWriter.UpdateArticleAsync(article, userId, isAdmin);

            if (updatedArticle == null)
            {
                return BadRequest("Failed to update article");
            }

            return NoContent();
        }

        /// <summary>
        /// Deletes an article
        /// </summary>
        /// <param name="id">The article id</param>
        /// <returns>No content if successful</returns>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Writer")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteArticle(int id)
        {
            var article = await _articleReader.GetArticleByIdAsync(id);
            if (article == null)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            if (!User.IsInRole("Admin") && article.AuthorId != userId)
            {
                return Forbid();
            }

            var result = await _articleWriter.DeleteArticleAsync(id);
            if (!result)
            {
                return BadRequest("Failed to delete article");
            }

            return NoContent();
        }

    }
}