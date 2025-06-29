using FluentValidation;
using FluentValidation.AspNetCore;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;

namespace UserService.Interceptor
{
    public class CustomValidatorInterceptor : IValidatorInterceptor
    {
        public IValidationContext BeforeAspNetValidation(ActionContext actionContext, IValidationContext validationContext)
        {
            return validationContext;
        }

        public ValidationResult AfterAspNetValidation(ActionContext actionContext, IValidationContext validationContext, ValidationResult result)
        {
            var filtered = result.Errors
                .Where(e => !string.IsNullOrWhiteSpace(e.ErrorMessage))
                .ToList();

            return new ValidationResult(filtered);
        }

        public IValidationContext BeforeMvcValidation(ControllerContext controllerContext, IValidationContext validationContext) => validationContext;
        public ValidationResult AfterMvcValidation(ControllerContext controllerContext, IValidationContext validationContext, ValidationResult result) => result;
    }

}
