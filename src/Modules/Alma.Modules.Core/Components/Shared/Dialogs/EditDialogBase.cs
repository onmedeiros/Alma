using Alma.Core.Types;
using Alma.Modules.Core.Components.Shared.Snackbars;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Alma.Modules.Core.Components.Shared.Dialogs
{
    public class EditDialogBase<T> : EditComponentBase<T>
    {
        [CascadingParameter]
        protected IMudDialogInstance Dialog { get; set; } = default!;

        protected override async ValueTask Result(ServiceResult<T> result)
        {
            IsSaving = false;
            StateHasChanged();

            if (result.Succeeded)
            {
                Snackbar.AddSuccess(SuccessMessage);
                Dialog.Close(DialogResult.Ok(result.Data));
                return;
            }

            await DialogService.ShowErrorDialog("Erro ao salvar");
        }
    }
}