using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Blog.Models
{
    public static class ArticleSeeder
    {
        public static async Task SeedDefaultArticleAsync(ApplicationDbContext context)
        {
            if (!await context.Articles.AnyAsync())
            {
                var adminUser = await context.Users.FirstOrDefaultAsync();

                if (adminUser != null)
                {
                    var article = new Article
                    {
                        Title = "Welcome to Our Blog",
                        Summary = "This is a sample article to get you started with our blog platform.",
                        Content = @"<p>Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat.</p>

<h2>What is Lorem Ipsum?</h2>
<p>Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nullam in dui mauris. Vivamus hendrerit arcu sed erat molestie vehicula. Sed auctor neque eu tellus rhoncus ut eleifend nibh porttitor. Ut in nulla enim. Phasellus molestie magna non est bibendum non venenatis nisl tempor. Suspendisse dictum feugiat nisl ut dapibus. Mauris iaculis porttitor posuere. Praesent id metus massa, ut blandit odio.</p>

<blockquote>
  Proin gravida nibh vel velit auctor aliquet. Aenean sollicitudin, lorem quis bibendum auctor, nisi elit consequat ipsum, nec sagittis sem nibh id elit.
</blockquote>

<h2>Duis ultrices volutpat aliquet?</h2>
<p>Pellentesque ullamcorper ullamcorper lectus, ac rhoncus nulla aliquam ac. Nam a nisl nulla. Fusce finibus tempus ipsum quis pretium. Donec mattis massa in enim pretium eleifend. Cras auctor gravida massa, non rutrum erat laoreet et. Integer nulla augue, faucibus sit amet metus scelerisque, tincidunt dictum dui.</p>

<h3>The standard Lorem Ipsum passage</h3>
<p>Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.</p>

<h3>Section with a list</h3>
<p>Here are some key benefits:</p>
<ul>
  <li>Lorem ipsum dolor sit amet, consectetur adipiscing elit.</li>
  <li>Quisque vehicula, libero eget bibendum bibendum.</li>
  <li>Morbi tempus neque vel molestie hendrerit.</li>
  <li>Donec consequat velit quis justo fermentum fermentum.</li>
</ul>

<p>Sed ut perspiciatis unde omnis iste natus error sit voluptatem accusantium doloremque laudantium, totam rem aperiam, eaque ipsa quae ab illo inventore veritatis et quasi architecto beatae vitae dicta sunt explicabo.</p>",
                        ImageUrl = "https://picsum.photos/800/400",
                        PublishedDate = DateTime.Now,
                        AuthorId = adminUser.Id
                    };

                    await context.Articles.AddAsync(article);
                    await context.SaveChangesAsync();
                }
            }
        }
    }
}