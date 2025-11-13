using Alma.Workflows.Core.Activities.Attributes;
using Alma.Workflows.Core.Activities.Base;
using Alma.Workflows.Databases.Common;

namespace Alma.Workflows.Databases.Activities
{
    [Activity(
        Namespace = "Alma.Workflows.Databases",
        Category = "Banco de dados",
        DisplayName = "Consulta ao Banco de Dados",
        Description = "Realiza uma consulta ao banco de dados com as informações configuradas.")]
    [ActivityCustomization(Icon = DatabaseIcons.Database, BorderColor = DatabaseColors.Default)]
    public class QueryDatabaseActivity : Activity
    {
    }
}