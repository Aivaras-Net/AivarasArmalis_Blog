using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Blog.Models;
using Blog.Services;
using Microsoft.Extensions.FileProviders;
using System.Runtime.Versioning;
using Blog.Controllers;

namespace Blog
{
    public class Program
    {
        [SupportedOSPlatform("windows")]
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite(connectionString));

            builder.Services.AddScoped<FileService>();
            builder.Services.AddScoped<InitialsProfileImageGenerator>();

            builder.Services.AddScoped<IArticleService, ArticleService>();
            builder.Services.AddScoped<IAccountService, AccountService>();
            builder.Services.AddScoped<IRoleService, RoleService>();
            builder.Services.AddScoped<IValidationService, ValidationService>();
            builder.Services.AddScoped<ICommentService, CommentService>();
            builder.Services.AddScoped<ICommentReportService, CommentReportService>();

            builder.Services.AddScoped<ILogger<ArticlesController>, Logger<ArticlesController>>();
            builder.Services.AddScoped<ILogger<AccountController>, Logger<AccountController>>();
            builder.Services.AddScoped<ILogger<RolesController>, Logger<RolesController>>();
            builder.Services.AddScoped<ILogger<HomeController>, Logger<HomeController>>();
            builder.Services.AddScoped<ILogger<CommentsController>, Logger<CommentsController>>();
            builder.Services.AddScoped<ILogger<CommentReportsController>, Logger<CommentReportsController>>();
            builder.Services.AddScoped<ILogger<CommentReportService>, Logger<CommentReportService>>();

            builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                builder.Configuration.GetSection("Identity:Password").Bind(options.Password);
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(
                    builder.Configuration.GetValue<int>("Identity:Lockout:DefaultLockoutTimeSpan"));
                options.Lockout.MaxFailedAccessAttempts =
                    builder.Configuration.GetValue<int>("Identity:Lockout:MaxFailedAccessAttempts");

                builder.Configuration.GetSection("Identity:User").Bind(options.User);
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

            builder.Services.ConfigureApplicationCookie(options =>
            {
                builder.Configuration.GetSection("Cookie").Bind(options);
                options.ExpireTimeSpan = TimeSpan.FromMinutes(
                    builder.Configuration.GetValue<int>("Cookie:ExpireTimeSpan"));
            });

            builder.Services.AddControllersWithViews();

            builder.Services.AddSingleton(EnvSettingsLoader.LoadEmailSettings());
            builder.Services.AddTransient<IEmailSender, EmailSender>();
            builder.Services.AddTransient<TemplateHelper>();

            builder.Logging.ClearProviders();
            builder.Logging.AddConsole();
            builder.Logging.AddDebug();

            var app = builder.Build();

            string dataFolder = Path.Combine(app.Environment.ContentRootPath, "data");
            Directory.CreateDirectory(dataFolder);

            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var context = services.GetRequiredService<ApplicationDbContext>();
                    context.Database.Migrate();

                    RoleSeeder.SeedRolesAsync(services).Wait();

                    var adminSettings = EnvSettingsLoader.LoadAdminSettings();

                    if (!string.IsNullOrEmpty(adminSettings.Email) && !string.IsNullOrEmpty(adminSettings.Password))
                    {
                        RoleSeeder.SeedAdminUserAsync(services, adminSettings.Email, adminSettings.Password).Wait();
                    }
                    ArticleSeeder.SeedDefaultArticleAsync(context).Wait();
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "An error occurred during application startup.");
                }
            }

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(Path.Combine(app.Environment.ContentRootPath, "data")),
                RequestPath = "/data"
            });

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
