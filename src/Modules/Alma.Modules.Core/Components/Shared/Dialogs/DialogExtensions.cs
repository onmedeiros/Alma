using Alma.Core.Types;
using Alma.Modules.Core.Components.Shared.Dialogs.Models;
using FluentValidation.Results;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Alma.Modules.Core.Components.Shared.Dialogs
{
    public static class DialogExtensions
    {
        public static Task<IDialogReference> ShowValidationError(this IDialogService dialogService, string title, string message)
        {
            var parameters = new DialogParameters<ValidationErrorDialog>
            {
                { x => x.Title, title },
                { x => x.Message, message }
            };

            return dialogService.ShowAsync<ValidationErrorDialog>(title, parameters);
        }

        public static Task<IDialogReference> ShowValidationError(this IDialogService dialogService, string title, ServiceResult result)
        {
            var errors = result.Errors?.Select(x => new ValidationError
            {
                Code = x.Code,
                Message = x.Message,
                PropertyName = x.Data != null && x.Data.ContainsKey("PropertyName") ? x.Data["PropertyName"].ToString() ?? string.Empty : string.Empty,
                AttemptedValue = x.Data != null && x.Data.ContainsKey("AttemptedValue") ? x.Data["AttemptedValue"]?.ToString() ?? string.Empty : string.Empty
            }).ToList();

            var parameters = new DialogParameters<ValidationErrorDialog>
            {
                { x => x.Title, title },
                { x => x.Message, result.Message },
                { x => x.Errors, errors }
            };

            return dialogService.ShowAsync<ValidationErrorDialog>(title, parameters);
        }

        public static Task<IDialogReference> ShowValidationError(this IDialogService dialogService, string title, ValidationFailure[] failures)
        {
            var errors = failures.Select(x => new ValidationError
            {
                Code = x.ErrorCode,
                Message = x.ErrorMessage,
                PropertyName = x.PropertyName,
                AttemptedValue = x.AttemptedValue?.ToString()
            }).ToList();

            var parameters = new DialogParameters<ValidationErrorDialog>
            {
                { x => x.Title, title },
                { x => x.Errors, errors }
            };

            return dialogService.ShowAsync<ValidationErrorDialog>(title, parameters);
        }

        public static Task<DialogResult?> ShowErrorDialog(this IDialogService dialogService, string message)
        {
            return dialogService.ShowErrorDialog("Erro", message);
        }

        public static async Task<DialogResult?> ShowErrorDialog(this IDialogService dialogService, string title, string message)
        {
            var parameters = new DialogParameters<ErrorDialog>
            {
                { x => x.Message, message }
            };

            var dialog = await dialogService.ShowAsync<ErrorDialog>(title, parameters);
            return await dialog.Result;
        }

        public static Task<DialogResult?> ShowWarningDialog(this IDialogService dialogService, string message)
        {
            return dialogService.ShowWarningDialog("Atenção!", message);
        }

        public static async Task<DialogResult?> ShowWarningDialog(this IDialogService dialogService, string title, string message)
        {
            var parameters = new DialogParameters<WarningDialog>
            {
                { x => x.Message, message }
            };

            var dialog = await dialogService.ShowAsync<WarningDialog>(title, parameters);
            return await dialog.Result;
        }

        public static async Task<DialogResult?> ShowDeleteConfirmationDialog(this IDialogService dialogService, string message)
        {
            var parameters = new DialogParameters<DeleteConfirmationDialog>
            {
                { x => x.Message, message }
            };

            var dialog = await dialogService.ShowAsync<DeleteConfirmationDialog>("Confirmação", parameters);

            return await dialog.Result;
        }

        public static Task<IDialogReference> ShowSelectAsync<TSelect>(this IDialogService dialogService)
            where TSelect : IComponent
        {
            var options = new DialogOptions()
            {
                MaxWidth = MaxWidth.Small,
                FullWidth = true,
                CloseButton = true,
                CloseOnEscapeKey = true,
                Position = DialogPosition.Center
            };

            return dialogService.ShowAsync<TSelect>("Selecione", options);
        }

        public static Task<IDialogReference> ShowDefaultAsync<TSelect>(this IDialogService dialogService, string title)
            where TSelect : IComponent
        {
            var options = new DialogOptions()
            {
                MaxWidth = MaxWidth.Small,
                FullWidth = true,
                CloseButton = true,
                CloseOnEscapeKey = true,
                Position = DialogPosition.Center
            };

            return dialogService.ShowAsync<TSelect>(title, options);
        }
    }
}