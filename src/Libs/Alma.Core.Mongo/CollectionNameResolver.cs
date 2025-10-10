namespace Alma.Core.Mongo
{
    internal static class CollectionNameResolver
    {
        public static string GetCollectionName(Type entityType)
        {
            // 1) Prefer custom Collection attribute if present
            var customCollectionAttr = entityType
                .GetCustomAttributes(inherit: true)
                .OfType<SimpleCore.Data.Mongo.Attributes.CollectionAttribute>()
                .FirstOrDefault();

            if (customCollectionAttr is not null && !string.IsNullOrWhiteSpace(customCollectionAttr.Name))
                return customCollectionAttr.Name;

            // 2) Support DataAnnotations TableAttribute (Name + optional Schema)
            var tableAttr = entityType
                .GetCustomAttributes(inherit: true)
                .OfType<System.ComponentModel.DataAnnotations.Schema.TableAttribute>()
                .FirstOrDefault();

            if (tableAttr is not null)
            {
                var name = tableAttr.Name;
                if (!string.IsNullOrWhiteSpace(tableAttr.Schema))
                    name = string.IsNullOrWhiteSpace(name) ? tableAttr.Schema : $"{tableAttr.Schema}.{name}";

                if (!string.IsNullOrWhiteSpace(name))
                    return name;
            }

            // 3) Fallback to type name
            return entityType.Name;
        }
    }
}