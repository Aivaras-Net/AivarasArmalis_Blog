using Blog.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Blog.Services.Comments
{
    public interface ICommentReportService
    {
        Task<IEnumerable<CommentReport>> GetAllReportsAsync();
        Task<IEnumerable<CommentReport>> GetPendingReportsAsync();
        Task<CommentReport?> GetReportByIdAsync(int id);
        Task<CommentReport?> CreateReportAsync(int commentId, string reason, string? reportDetails, string reporterId);
        Task<CommentReport?> UpdateReportStatusAsync(int id, ReportStatus status, string? notes, string reviewerId);
        Task<bool> HasUserReportedCommentAsync(int commentId, string userId);
        Task<Comment?> BlockCommentAsync(int commentId, string blockReason, string adminId);
        Task<Comment?> UnblockCommentAsync(int commentId, string adminId);
    }

    public class CommentReportService : ICommentReportService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<CommentReportService> _logger;

        public CommentReportService(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            ILogger<CommentReportService> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<IEnumerable<CommentReport>> GetAllReportsAsync()
        {
            return await _context.CommentReports
                .Include(r => r.Comment)
                    .ThenInclude(c => c.Author)
                .Include(r => r.Reporter)
                .Include(r => r.Reviewer)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<CommentReport>> GetPendingReportsAsync()
        {
            return await _context.CommentReports
                .Include(r => r.Comment)
                    .ThenInclude(c => c.Author)
                .Include(r => r.Reporter)
                .Where(r => r.Status == ReportStatus.Pending)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<CommentReport?> GetReportByIdAsync(int id)
        {
            return await _context.CommentReports
                .Include(r => r.Comment)
                    .ThenInclude(c => c.Author)
                .Include(r => r.Reporter)
                .Include(r => r.Reviewer)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<CommentReport?> CreateReportAsync(int commentId, string reason, string? reportDetails, string reporterId)
        {
            try
            {
                var comment = await _context.Comments.FindAsync(commentId);
                if (comment == null)
                {
                    _logger.LogWarning("Attempted to report non-existent comment {CommentId}", commentId);
                    return null;
                }

                bool hasReported = await HasUserReportedCommentAsync(commentId, reporterId);
                if (hasReported)
                {
                    _logger.LogInformation("User {UserId} has already reported comment {CommentId}", reporterId, commentId);
                    return null;
                }

                var report = new CommentReport
                {
                    CommentId = commentId,
                    ReporterId = reporterId,
                    Reason = reason,
                    ReportDetails = reportDetails,
                    CreatedAt = DateTime.Now,
                    Status = ReportStatus.Pending
                };

                _context.CommentReports.Add(report);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Comment {CommentId} reported by user {UserId}", commentId, reporterId);

                return await GetReportByIdAsync(report.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating report for comment {CommentId}", commentId);
                return null;
            }
        }

        public async Task<CommentReport?> UpdateReportStatusAsync(int id, ReportStatus status, string? notes, string reviewerId)
        {
            try
            {
                var report = await _context.CommentReports.FindAsync(id);
                if (report == null)
                {
                    _logger.LogWarning("Attempted to update non-existent report {ReportId}", id);
                    return null;
                }

                report.Status = status;
                report.ReviewNotes = notes;
                report.ReviewerId = reviewerId;
                report.ReviewedAt = DateTime.Now;

                _context.CommentReports.Update(report);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Report {ReportId} updated to status {Status} by reviewer {ReviewerId}",
                    id, status, reviewerId);

                return await GetReportByIdAsync(report.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating report {ReportId}", id);
                return null;
            }
        }

        public async Task<bool> HasUserReportedCommentAsync(int commentId, string userId)
        {
            return await _context.CommentReports
                .AnyAsync(r => r.CommentId == commentId && r.ReporterId == userId);
        }

        public async Task<Comment?> BlockCommentAsync(int commentId, string blockReason, string adminId)
        {
            try
            {
                var comment = await _context.Comments.FindAsync(commentId);
                if (comment == null)
                {
                    _logger.LogWarning("Attempted to block non-existent comment {CommentId}", commentId);
                    return null;
                }

                comment.IsBlocked = true;
                comment.BlockedAt = DateTime.Now;
                comment.BlockedById = adminId;
                comment.BlockReason = blockReason;

                _context.Comments.Update(comment);

                var pendingReports = await _context.CommentReports
                    .Where(r => r.CommentId == commentId && r.Status == ReportStatus.Pending)
                    .ToListAsync();

                foreach (var report in pendingReports)
                {
                    report.Status = ReportStatus.ActionTaken;
                    report.ReviewerId = adminId;
                    report.ReviewedAt = DateTime.Now;
                    report.ReviewNotes = $"Comment blocked. Reason: {blockReason}";
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Comment {CommentId} blocked by admin {AdminId}", commentId, adminId);

                return await _context.Comments
                    .Include(c => c.Author)
                    .Include(c => c.BlockedBy)
                    .FirstOrDefaultAsync(c => c.Id == commentId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error blocking comment {CommentId}", commentId);
                return null;
            }
        }

        public async Task<Comment?> UnblockCommentAsync(int commentId, string adminId)
        {
            try
            {
                var comment = await _context.Comments.FindAsync(commentId);
                if (comment == null)
                {
                    _logger.LogWarning("Attempted to unblock non-existent comment {CommentId}", commentId);
                    return null;
                }

                comment.IsBlocked = false;
                comment.BlockedAt = null;
                comment.BlockedById = null;
                comment.BlockReason = null;

                _context.Comments.Update(comment);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Comment {CommentId} unblocked by admin {AdminId}", commentId, adminId);

                return await _context.Comments
                    .Include(c => c.Author)
                    .FirstOrDefaultAsync(c => c.Id == commentId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unblocking comment {CommentId}", commentId);
                return null;
            }
        }
    }
}