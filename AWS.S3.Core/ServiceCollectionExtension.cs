using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AWS.S3.Core
{
    public static class ServiceCollectionExtension
    {
        public static IServiceCollection AddS3Config(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<S3ConfigOptions>(
                configuration.GetSection(nameof(S3ConfigOptions)));
            return services;
        }

        public static IServiceCollection AddS3Services(this IServiceCollection services)
        {
            services.AddSingleton<IAmazonS3ClientFactory, AmazonS3ClientFactory>();
            services.AddScoped<IAmazonS3Service, AmazonS3Service>();            
            return services;
        }
    }
}
