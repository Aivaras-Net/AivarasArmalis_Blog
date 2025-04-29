namespace Blog.Services.Comments.Interfaces
{
    /// <summary>
    /// Combined service interface for backward compatibility
    /// </summary>
    public interface ICommentService : ICommentReader, ICommentManager { }
}