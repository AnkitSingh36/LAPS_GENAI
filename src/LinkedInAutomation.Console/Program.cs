using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using LinkedInAutomation.Core.Services;
using Microsoft.Extensions.Logging;
using System.Net.Http;

namespace LinkedInAutomation.Console
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var services = ConfigureServices();
            var generator = services.GetRequiredService<IAIContentGenerator>();
            var telegram = services.GetRequiredService<ITelegramService>();
            var linkedin = services.GetRequiredService<ILinkedInService>();

            try
            {
                // Ask for topic via Telegram
                var topic = await telegram.AskForTopic();

                // Generate content
                var content = await generator.GenerateContentAsync(topic);

                // Get approval via Telegram
                if (await telegram.RequestApproval(content))
                {
                    // Post to LinkedIn
                    await linkedin.PostContent(content);
                    await telegram.SendMessage("Post published successfully!");
                }
                else
                {
                    await telegram.SendMessage("Post cancelled.");
                }
            }
            catch (Exception ex)
            {
                await telegram.SendMessage($"Error: {ex.Message}");
                throw;
            }
        }

        public static IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();

            var configuration = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .Build();

            services.AddSingleton<IAIContentGenerator>(new HuggingFaceGenerator(
                Environment.GetEnvironmentVariable("HUGGINGFACE_API_TOKEN") ?? throw new InvalidOperationException("HuggingFace API Token not found.")
            ));

            services.AddSingleton<ITelegramService>(provider =>
            {
                var logger = provider.GetRequiredService<ILogger<TelegramService>>();

                return new TelegramService(
                    Environment.GetEnvironmentVariable("TELEGRAM_BOT_TOKEN") ?? throw new InvalidOperationException("Telegram Bot Token not found."),
                    long.Parse(Environment.GetEnvironmentVariable("TELEGRAM_CHAT_ID") ?? throw new InvalidOperationException("Telegram Chat ID not found.")),
                    logger
                );
            });
            services.AddHttpClient<ILinkedInService, LinkedInService>();

            services.AddTransient<ILinkedInService>(provider =>
               {
                   var logger = provider.GetRequiredService<ILogger<LinkedInService>>();
                   return new LinkedInService(
                       new HttpClient(),
                       Environment.GetEnvironmentVariable("LINKEDIN_CLIENT_ID") ?? throw new InvalidOperationException("LinkedIn Client ID not found."),
                       Environment.GetEnvironmentVariable("LINKEDIN_CLIENT_SECRET") ?? throw new InvalidOperationException("LinkedIn Client Secret not found."),
                       logger
                   );
               });

            return services.BuildServiceProvider();
        }
    }
}
