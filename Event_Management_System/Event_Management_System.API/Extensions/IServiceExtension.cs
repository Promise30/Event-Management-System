namespace Event_Management_System.API.Extensions
{
    public static class IServiceExtension
    {
        public static void ConfigureHttpClients(this IServiceCollection services, IConfiguration configuration)
        {
            // Example of configuring a named HttpClient
            services.AddHttpClient("Flutterwave", httpClient =>
            {
                httpClient.BaseAddress = new Uri(configuration["Flutterwave:BaseUrl"]);
                httpClient.DefaultRequestHeaders.Add("Content-Type", "application/json");
                httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {configuration["Flutterwave:ApiKey"]}");
            });
            // Extension methods for IServiceCollection can be added here in the future
            // custom named httpclient config
        }

    }
}
