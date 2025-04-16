using Blog.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Blog.Services
{
    public interface IValidationService
    {
        bool ValidateArticle(Article article, ModelStateDictionary modelState);
        bool ValidateUserEmailUpdate(string email, ModelStateDictionary modelState);
        bool ValidateUserNameUpdate(string firstName, string lastName, ModelStateDictionary modelState);
        bool ValidatePasswordReset(ResetPasswordViewModel model, ModelStateDictionary modelState);
    }

    public class ValidationService : IValidationService
    {
        public bool ValidateArticle(Article article, ModelStateDictionary modelState)
        {
            modelState.Remove("AuthorId");

            if (string.IsNullOrWhiteSpace(article.Title))
            {
                modelState.AddModelError("Title", "Title is required");
                return false;
            }

            // Remove validation for optional fields
            modelState.Remove("Summary");
            modelState.Remove("Content");
            modelState.Remove("ImageUrl");

            return modelState.IsValid;
        }

        public bool ValidateUserEmailUpdate(string email, ModelStateDictionary modelState)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                modelState.AddModelError("Email", "Email is required");
                return false;
            }

            // Email format validation
            if (!email.Contains('@') || !email.Contains('.'))
            {
                modelState.AddModelError("Email", "Invalid email format");
                return false;
            }

            return true;
        }

        public bool ValidateUserNameUpdate(string firstName, string lastName, ModelStateDictionary modelState)
        {
            if (string.IsNullOrWhiteSpace(firstName))
            {
                modelState.AddModelError("FirstName", "First name is required");
                return false;
            }

            if (string.IsNullOrWhiteSpace(lastName))
            {
                modelState.AddModelError("LastName", "Last name is required");
                return false;
            }

            return true;
        }

        public bool ValidatePasswordReset(ResetPasswordViewModel model, ModelStateDictionary modelState)
        {
            if (string.IsNullOrWhiteSpace(model.Password))
            {
                modelState.AddModelError("Password", "Password is required");
                return false;
            }

            if (model.Password != model.ConfirmPassword)
            {
                modelState.AddModelError("ConfirmPassword", "The password and confirmation password do not match");
                return false;
            }

            return modelState.IsValid;
        }
    }
}