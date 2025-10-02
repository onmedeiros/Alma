using Blazored.FluentValidation;
using Alma.Core.Types;
using Alma.Modules.Core.Components.Shared.Dialogs;
using Alma.Modules.Core.Components.Shared.Snackbars;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;

namespace Alma.Modules.Core.Components.Shared
{
    public class EditComponentBase<T> : ComponentBase
    {
        [Inject]
        protected IDialogService DialogService { get; set; } = default!;

        [Inject]
        protected ISnackbar Snackbar { get; set; } = default!;

        [Inject]
        protected IServiceScopeFactory ScopeFactory { get; set; } = default!;

        protected FluentValidationValidator Validator { get; set; } = default!;

        protected bool IsSaving { get; set; }

        protected string SuccessMessage { get; set; } = "Informações salvas com sucesso.";

        protected virtual async ValueTask<bool> Validate()
        {
            if (!await Validator.ValidateAsync())
            {
                IsSaving = false;
                StateHasChanged();

                var failures = Validator.GetFailuresFromLastValidation();

                await DialogService.ShowValidationError("Erro de validação", failures);

                return false;
            }

            return true;
        }

        protected virtual async ValueTask<bool> TrySave(Func<ValueTask<ServiceResult<T>>> action)
        {
            IsSaving = true;
            StateHasChanged();

            if (!await Validate())
                return false;

            try
            {
                var result = await action();
                await Result(result);

                return true;
            }
            catch (Exception ex)
            {
                IsSaving = false;
                StateHasChanged();

                await DialogService.ShowErrorDialog("Não foi possível salvar as alterações", ex.Message);
                return false;
            }
        }

        protected virtual async ValueTask Result(ServiceResult<T> result)
        {
            IsSaving = false;
            StateHasChanged();

            if (result.Succeeded)
            {
                Snackbar.AddSuccess(SuccessMessage);
                return;
            }
            if (result.Status == ServiceResultStatus.WarningError)
            {
                await DialogService.ShowWarningDialog("Não foi possível salvar as alterações", result.Message ?? "Erro desconhecido.");
                return;
            }

            await DialogService.ShowErrorDialog(result.Message ?? "Erro ao salvar.");
        }
    }
}