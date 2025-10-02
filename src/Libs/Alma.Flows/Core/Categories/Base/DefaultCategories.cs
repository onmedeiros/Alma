using Alma.Flows.Core.Categories.Entities;

namespace Alma.Flows.Core.Categories.Base
{
    public static class DefaultCategories
    {
        public static readonly Category Data = new() { Id = "system.data", ResourceName = "Alma_Category_Data", DefaultName = "Dados", IsSystemDefault = true };
        public static readonly Category Flow = new() { Id = "system.flow", ResourceName = "Alma_Category_Flow", DefaultName = "Fluxo", IsSystemDefault = true };
        public static readonly Category Integration = new() { Id = "system.integration", ResourceName = "Alma_Category_Integration", DefaultName = "Integração", IsSystemDefault = true };
        public static readonly Category Interaction = new() { Id = "system.interaction", ResourceName = "Alma_Category_Interaction", DefaultName = "Interação", IsSystemDefault = true };
        public static readonly Category Others = new() { Id = "system.others", ResourceName = "Alma_Category_Others", DefaultName = "Outras", IsSystemDefault = true };

        public static readonly IReadOnlyList<Category> All = new List<Category>
        {
            Data,
            Flow,
            Integration,
            Interaction,
            Others
        };
    }
}