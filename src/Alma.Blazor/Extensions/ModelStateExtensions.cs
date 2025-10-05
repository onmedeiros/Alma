using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Alma.Core.Types;

namespace Alma.Blazor.Extensions
{
    public static class ModelStateExtensions
    {
        //public static ModelStateDictionary AddModelError(this ModelStateDictionary modelState, ValidationResult? result)
        //{
        //    if (result != null)
        //    {
        //        foreach (var error in result!.Errors)
        //        {
        //            modelState.AddModelError(error.PropertyName, error.ErrorMessage);
        //        }
        //    }

        //    return modelState;
        //}

        //public static ModelStateDictionary AddModelError(this ModelStateDictionary modelState, ServiceResult result)
        //{
        //    if (result.Errors != null)
        //    {
        //        foreach (var error in result.Errors)
        //        {
        //            modelState.AddModelError(error.Key, error.Value);
        //        }
        //    }
        //    else if (result.ValidationResult != null)
        //    {
        //        foreach (var error in result.ValidationResult.Errors)
        //        {
        //            modelState.AddModelError(error.PropertyName, error.ErrorMessage);
        //        }
        //    }
        //    else
        //    {
        //        modelState.AddModelError(string.Empty, "Erro de validação.");
        //    }

        //    return modelState;
        //}
    }
}