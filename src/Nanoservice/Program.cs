using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Nanoservice
{
    public class Program
    {
        public static async Task Main(string[] args) => await WebHost.CreateDefaultBuilder(args)
            .ConfigureServices(services =>
            {
                IConfiguration configuration;
                using (var serviceProvider = services.BuildServiceProvider())
                {
                    configuration = serviceProvider.GetService<IConfiguration>();
                }

                services.AddRouting()
                    .AddHttpClient()
                    .Configure<AppOptions>(configuration.GetSection("app"));
            })
            .Configure(app =>
            {
                var id = Guid.NewGuid().ToString("N");
                app.ApplicationServices.GetService<ILogger<Program>>().LogInformation($"Nanoservice id: {id}");
                var appOptions = app.ApplicationServices.GetService<IOptions<AppOptions>>().Value;
                var message = GetOption(nameof(appOptions.Message), appOptions.Message);
                var file = GetOption(nameof(appOptions.File), appOptions.File);
                var nextServiceUrl = GetOption(nameof(appOptions.NextServiceUrl), appOptions.NextServiceUrl);

                app.UseDeveloperExceptionPage();
                app.UseRouter(router =>
                {
                    router.MapGet("", (request, response, data) => response.WriteAsync(message));
                    router.MapGet("id", (request, response, data) => response.WriteAsync(id));
                    router.MapGet("file", (request, response, data) =>
                        response.WriteAsync(File.Exists(file)
                            ? File.ReadAllText(file)
                            : $"File: '{file}' was not found."));
                    router.MapGet("next", async (request, response, data) =>
                    {
                        var httpClient = app.ApplicationServices.GetService<IHttpClientFactory>().CreateClient();
                        var nextMessage = await httpClient.GetStringAsync(nextServiceUrl);
                        await response.WriteAsync($"{message} -> {nextMessage}");
                    });

                });
            })
            .Build()
            .RunAsync();

        private static string GetOption(string property, string value)
            => Environment.GetEnvironmentVariable($"NANO_{property.ToUpperInvariant()}") ?? value;

        private class AppOptions
        {
            public string Message { get; set; }
            public string File { get; set; }
            public string NextServiceUrl { get; set; }
        }
    }
}
