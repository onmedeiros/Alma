using System.ComponentModel;

namespace Alma.Core.Modules
{
    public enum ModuleCategory
    {
        [Description("Início")]
        Home = 1,

        [Description("Gerenciamento")]
        Management = 10,

        [Description("Processamento")]
        Processing = 30,

        [Description("Relatórios")]
        Reports = 40,

        [Description("Outros")]
        Others = 50,

        [Description("Configurações")]
        Settings = 100
    }
}
