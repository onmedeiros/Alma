using Alma.Core.Attributes;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;

namespace Alma.Core.Mongo.Conventions
{
    public class IgnoreNavigationConvention : ConventionBase, IMemberMapConvention
    {
        public void Apply(BsonMemberMap memberMap)
        {
            var hasNavigationAttribute = memberMap.MemberInfo
                .GetCustomAttributes(typeof(NavigationAttribute), true)
                .Any();

            if (hasNavigationAttribute)
                memberMap.SetShouldSerializeMethod(_ => false);
        }
    }
}