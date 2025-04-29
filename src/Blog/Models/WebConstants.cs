namespace Blog.Models
{
    public static class WebConstants
    {
        public static string ProfilePictureUpdated = "Your profile picture has been updated";
        public static string ProfilePictureUpdateError = "Error changing profile picture";
        public static string EmailUpdated = "Your email has been updated";
        public static string NameUpdated = "Your name has been updated";
        public static string PasswordChanged = "Your password has been changed";
        public static string InvalidLoginAttempt = "Invalid login attempt.";
        public static string PasswordResetEmailError = "There was an error sending the password reset email. Please try again.";
        public static string InvalidPasswordResetCode = "Invalid password reset code or user ID.";
        public static string PasswordResetEmailSubject = "Reset Your Password";

        public static string ArticleCreationError = "An error occurred while saving the article. Please try again.";
        public static string ArticleUpdateError = "An error occurred while updating the article. Please try again.";
        public static string ArticleDeleteSuccess = "Article deleted successfully";
        public static string ArticleDeleteError = "Failed to delete article with ID {0}";
        public static string ArticleVoteError = "Failed to vote on the article";
        public static string ArticleRemoveVoteError = "Failed to remove vote from the article";

        public static string CommentReportReasonEmpty = "Report reason cannot be empty";
        public static string CommentReportSuccess = "Comment reported successfully";
        public static string CommentReportError = "An error occurred while reporting the comment";
        public static string CommentContentEmpty = "Comment content cannot be empty";
        public static string CommentCreationError = "An error occurred while creating the comment";
        public static string CommentUpdateError = "Comment not found or you don't have permission to edit it";
        public static string CommentDeleteSuccess = "Comment deleted successfully";
        public static string CommentDeleteError = "An error occurred while deleting the comment";
        public static string CommentNotFound = "Comment not found";
        public static string CommentAlreadyReported = "You have already reported this comment";
        public static string CommentReportReviewed = "Report reviewed successfully";
        public static string CommentBlocked = "This comment has been blocked and cannot be edited";
        public static string CommentBlockedAdminOnly = "This comment has been blocked and can only be deleted by an administrator";
        public static string CommentUnblocked = "Comment unblocked successfully";
        public static string CommentBlockSuccess = "Comment blocked successfully";

        public static string SearchTermRequired = "Search term is required";

        public static string AdminRoleProtected = "You cannot remove your own Admin role. Other roles updated successfully.";
        public static string RolesUpdated = "Roles updated successfully.";
        public static string RolesUpdateFailed = "Failed to update user roles.";
        public static string AdminRoleName = "Admin";
        public static string UserRoleName = "User";

        public static string TitleRequired = "Title is required";
        public static string EmailRequired = "Email is required";
        public static string InvalidEmailFormat = "Invalid email format";
        public static string FirstNameRequired = "First name is required";
        public static string LastNameRequired = "Last name is required";
        public static string PasswordRequired = "Password is required";
        public static string PasswordsDoNotMatch = "The password and confirmation password do not match";

        public static string InitialsImagePath = "data/UserImages/initials/";

        public static string NotFound = "Not found";
        public static string UserNotFound = "Unable to load user with ID '{0}'.";

        public static string LogArticleCreateCalled = "Create POST called with article: Title={0}, SummaryLength={1}, ContentLength={2}";
        public static string LogArticleEditCalled = "Edit POST called with article ID: {0}, SummaryLength: {1}, ContentLength: {2}";
        public static string LogArticleCreated = "Article created successfully with ID: {0}";
        public static string LogArticleUpdated = "Article updated successfully";
        public static string LogSetAuthorId = "Setting AuthorId to current user: {0}";
        public static string LogIdMismatch = "ID mismatch: {0} vs {1}";
        public static string LogValidationError = "Validation error for {0}: {1}";
        public static string LogVoteFailed = "Vote failed for article {0}";
        public static string LogRemoveVoteFailed = "Remove vote failed for article {0}";
        public static string LogCreatingArticle = "Creating article: Title={0}";
        public static string LogArticleNotFound = "Article with ID {0} not found";
        public static string LogUnauthorizedEdit = "User tried to edit an article they don't own";
        public static string LogErrorCreatingArticle = "Error creating article";
        public static string LogErrorUpdatingArticle = "Error updating article {0}";
        public static string LogErrorDeletingArticle = "Error deleting article {0}";
        public static string LogErrorVoting = "Error voting on article {0}";
        public static string LogErrorRemovingVote = "Error removing vote from article {0}";

        public static string LogRolesUpdateFailed = "Failed to update roles for user {0}";
        public static string LogFailedToRemoveRoles = "Failed to remove roles from user {0}";

        public static string LogPasswordResetEmailFailed = "Failed to send password reset email to {0}";
        public static string LogProfilePictureUpdateError = "Error updating profile picture for user {0}";

        public static string LogSendingEmail = "Sending email to {0}";
        public static string LogEmailSent = "Email sent to {0}";
        public static string LogEmailSendingFailed = "Failed to send email: {0}";
    }
}