using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using Serilog;
using Alma.Core.Data.Mongo.Conventions;

namespace SimpleCore.Data.Mongo
{
    public static class MongoExtensions
    {
        public static IServiceCollection AddSimpleMongo<TContext>(this IServiceCollection services, Action<SimpleMongoOptions> options)
        {
            Log.Information("Adding SimpleMongo.");

            services.Configure(options);

            services.AddSingleton<IMongoClient, MongoClient>(sp =>
            {
                var options = sp.GetRequiredService<IOptions<SimpleMongoOptions>>().Value;
                return new MongoClient(options.ConnectionString);
            });

            services.AddScoped(typeof(IMongoRepository<>), typeof(MongoRepository<>));

            services.AddSingleton(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<MongoDbContext>>();
                var client = sp.GetRequiredService<IMongoClient>();
                var options = sp.GetRequiredService<IOptions<SimpleMongoOptions>>().Value;

                return new MongoDbContext(logger, client, options.ConnectionString, options.Database);
            });

            services.AddHostedService<MongoIndexCreator>();

            return services;
        }
    }
}