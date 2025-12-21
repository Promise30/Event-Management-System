using Event_Management_System.API.Application;

namespace Event_Management_System.API.Extensions
{
    public class CacheSettings
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public string Password { get; set; }
        public string ConnectionString => $"{Host}:{Port},password={Password}";
    }
    public static class RedisConfigExtension
    {
        public static IServiceCollection AddRedisCache(this IServiceCollection services, IConfiguration configuration)
        {
            var cacheSettings = configuration.GetSection(nameof(CacheSettings)).Get<CacheSettings>();
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = cacheSettings.ConnectionString;
            });
            services.Configure<CacheSettings>(configuration.GetSection(nameof(CacheSettings)));
            services.AddSingleton<IRedisService, RedisService>();
            return services;
        }

    }
}
