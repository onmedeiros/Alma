using Alma.Flows.Design.Components.Shared;
using MudBlazor;

namespace Alma.Flows.Design.Extensions
{
    public static class DialogServiceExtensions
    {
        public static async Task<DialogResult?> ShowDeleteConfirmationDialog(this IDialogService dialogService, string message)
        {
            var parameters = new DialogParameters<DeleteConfirmationDialog>
            {
                { x => x.Message, message }
            };

            var dialog = await dialogService.ShowAsync<DeleteConfirmationDialog>("Confirmação", parameters);

            return await dialog.Result;
        }
    }
}
