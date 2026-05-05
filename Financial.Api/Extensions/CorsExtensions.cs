namespace Financial.Api.Extensions
{
    public static class CorsExtensions
    {
        public static void AddCustomCors(this IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("AllowDev", builder =>
                {
                    builder
                        .WithOrigins("http://localhost:4200", "http://localhost:3000", "http://localhost:5001", "https://localhost:5002")
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                });
            });
        }
    }
}
