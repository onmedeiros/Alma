using MudBlazor;

namespace Alma.Modules.Core.Components.Shared.Snackbars
{
    public static class SnackbarExtensions
    {
        public static bool IsConfigured = false;

        public static Action<SnackbarOptions> Options = options =>
        {
            options.SnackbarVariant = Variant.Filled;
            options.ShowTransitionDuration = 200;
            options.HideTransitionDuration = 200;
        };

        public static void AddSuccess(this ISnackbar snackbar, string message)
        {
            Configure(snackbar);
            snackbar.Add(message, Severity.Success, Options);
        }

        public static void AddError(this ISnackbar snackbar, string message)
        {
            Configure(snackbar);
            snackbar.Add(message, Severity.Error, Options);
        }

        public static void AddWarning(this ISnackbar snackbar, string message)
        {
            Configure(snackbar);
            snackbar.Add(message, Severity.Warning, Options);
        }

        public static void Configure(ISnackbar snackbar)
        {
            snackbar.Configuration.PositionClass = Defaults.Classes.Position.TopCenter;
            IsConfigured = true;
        }
    }
}
