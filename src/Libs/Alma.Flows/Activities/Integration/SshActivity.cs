using Alma.Flows.Core.Activities.Attributes;
using Alma.Flows.Core.Activities.Base;
using Alma.Flows.Core.Contexts;
using Alma.Flows.Customizations;
using Renci.SshNet;

namespace Alma.Flows.Activities.Integration
{
    [Activity(
        Namespace = "Alma.Flows",
        Category = "Integração",
        DisplayName = "SSH",
        Description = "Executa comandos através de uma conexão SSH.")]
    [ActivityCustomization(Icon = FlowIcons.Terminal, BorderColor = FlowColors.Integration)]
    public class SshActivity : Activity
    {
        #region Ports

        [Port(DisplayName = "Entrada", Type = PortType.Input)]
        public Port Input { get; set; } = default!;

        [Port(DisplayName = "Sucesso", Type = PortType.Output)]
        [PortCustomization(Color = FlowColors.Success)]
        public Port Success { get; set; } = default!;

        [Port(DisplayName = "Erro", Type = PortType.Output)]
        [PortCustomization(Color = FlowColors.Error)]
        public Port Error { get; set; } = default!;

        #endregion

        #region Parameters

        [ActivityParameter(DisplayName = "Host", DisplayValue = "Host: {{value}}")]
        public Parameter<string>? Host { get; set; }

        [ActivityParameter(DisplayName = "Username", DisplayValue = "UserName: {{value}}")]
        public Parameter<string>? Username { get; set; }

        [ActivityParameter(DisplayName = "Password")]
        public Parameter<string>? Password { get; set; }

        [ActivityParameter(DisplayName = "Comandos", AutoGrow = true, Lines = 3, MaxLines = 8)]
        public Parameter<string>? Commands { get; set; }

        #endregion

        public override async ValueTask ExecuteAsync(ActivityExecutionContext context)
        {
            using var client = new SshClient(Host?.GetValue(context) ?? string.Empty, Username?.GetValue(context) ?? string.Empty, Password?.GetValue(context) ?? string.Empty);

            try
            {
                await client.ConnectAsync(CancellationToken.None);
                using var command = client.CreateCommand(Commands?.GetValue(context) ?? string.Empty);
                await command.ExecuteAsync(CancellationToken.None);
                Success.Execute(command.Result);
            }
            catch (Exception ex)
            {
                Error.Execute(ex.Message);
            }
            finally
            {
                if (client.IsConnected)
                {
                    client.Disconnect();
                }
            }
        }
    }
}