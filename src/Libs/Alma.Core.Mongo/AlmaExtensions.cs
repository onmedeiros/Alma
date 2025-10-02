using Alma.Core.Data;
using Alma.Core.Entities;
using Alma.Core.Mongo.Conventions;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;

namespace Alma.Core.Mongo
{
    public static class AlmaExtensions
    {
        private static bool _mappingsRegistered;

        /// <summary>
        /// Registers MongoDB conventions and class mappings for Alma core entities.
        /// Call this once during application startup before using MongoDB.
        /// </summary>
        public static void RegisterMongoMappings()
        {
            if (_mappingsRegistered)
                return;

            // Conventions (apply to all mapped classes)
            var pack = new ConventionPack
            {
                new EnumRepresentationConvention(BsonType.String)
            };

            ConventionRegistry.Register("Alma.Core.Mongo.Conventions", pack, _ => true);

            // Entity mapping (Id as _id, represented as string)
            if (!BsonClassMap.IsClassMapRegistered(typeof(Entity)))
            {
                BsonClassMap.RegisterClassMap<Entity>(cm =>
                {
                    cm.AutoMap();
                    cm.MapIdProperty(e => e.Id)
                        .SetSerializer(new StringSerializer(BsonType.String));
                });
            }

            _mappingsRegistered = true;
        }

        public static IServiceCollection AddAlmaMongo(this IServiceCollection services, string connectionString, string databaseName)
        {
            ArgumentException.ThrowIfNullOrEmpty(connectionString);
            ArgumentException.ThrowIfNullOrEmpty(databaseName);

            services.AddSingleton<IMongoClient>(new MongoClient(connectionString));
            services.AddSingleton(provider =>
            {
                var client = provider.GetRequiredService<IMongoClient>();
                return client.GetDatabase(databaseName);
            });

            services.AddScoped<MongoContext>();
            services.AddScoped<IContext, MongoContext>();
            services.AddScoped(typeof(IRepository<>), typeof(MongoRepository<>));

            BsonSerializer.RegisterSerializer(typeof(decimal), new DecimalSerializer(BsonType.Decimal128));
            BsonSerializer.RegisterSerializer(typeof(decimal?), new NullableSerializer<decimal>(new DecimalSerializer(BsonType.Decimal128)));

            // Cria um pack de convenções padrão para projetos Simple Core
            var conventionPack = new ConventionPack {
                new EnumRepresentationConvention(BsonType.String),
                new IgnoreExtraElementsConvention(true),
                new IgnoreNavigationConvention()
            };

            // Registra a convenção para todas as classes (o predicado "t => true" aplica para todas)
            ConventionRegistry.Register("AlmaConventions", conventionPack, t => true);

            RegisterMongoMappings();

            return services;
        }
    }
}