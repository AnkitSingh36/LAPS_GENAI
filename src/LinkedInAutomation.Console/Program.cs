using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using LinkedInAutomation.Core.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using System.Text.Json;

namespace LinkedInAutomation.Console
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            //var services = ConfigureServices();
            var config = LoadConfiguration();

            var host = Host.CreateDefaultBuilder(args)
                .ConfigureServices((context, services) =>
                {
                    services.AddHttpClient();
                    services.AddSingleton<ITelegramService>(provider =>
                    {
                        var logger = provider.GetRequiredService<ILogger<TelegramService>>();
                        return new TelegramService(config.TelegramBotToken ?? throw new InvalidOperationException("TelegramBotToken not found."), long.Parse(config.TelegramChatId), logger);
                    });
                    services.AddHttpClient<IAIContentGenerator, HuggingFaceGenerator>();
                    services.AddSingleton<IAIContentGenerator>(new HuggingFaceGenerator(new HttpClient(), config.HuggingFaceApiKey ?? throw new InvalidOperationException("HuggingFaceApiKey not found.")));
                    services.AddHttpClient<ILinkedinService, LinkedInService>();
                    services.AddSingleton<ILinkedinService>(provider =>
                    {
                        var logger = provider.GetRequiredService<ILogger<LinkedInService>>();
                        return new LinkedInService(logger, config.LinkedInUSERID ?? throw new InvalidOperationException("LinkedIn User ID not found."), config.LinkedSECRETKEY ?? throw new InvalidOperationException("LinkedIn Secret Id not found."));
                        //return new LinkedInService(logger, Environment.GetEnvironmentVariable("LINKEDIN_EMAIL") ?? throw new ArgumentNullException("Missing LinkedIn Email"), Environment.GetEnvironmentVariable("LINKEDIN_PASSWORD") ?? throw new ArgumentNullException("Missing LinkedIn Password"));

                    });
                })
                .Build();

            var generator = host.Services.GetRequiredService<IAIContentGenerator>();
            var telegram = host.Services.GetRequiredService<ITelegramService>();
            var linkedin = host.Services.GetRequiredService<ILinkedinService>();

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
                    if( await linkedin.PostContentAsync(content)) 
                    { 
                        await telegram.SendMessage("Post published successfully!"); 
                    } else 
                    { 
                        await telegram.SendMessage("Post cancelled."); 
                    }
                        
                }
                else
                {
                    await telegram.SendMessage("Post rejected.");
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

            services.AddHttpClient();
            services.AddSingleton<ITelegramService>(provider =>
            {
                var logger = provider.GetRequiredService<ILogger<TelegramService>>();
                return new TelegramService(Environment.GetEnvironmentVariable("TELEGRAM_BOT_TOKEN") ?? throw new InvalidOperationException("TelegramBotToken not found."), long.Parse(Environment.GetEnvironmentVariable("TELEGRAM_CHAT_ID")!), logger);
            });
            services.AddHttpClient<IAIContentGenerator, HuggingFaceGenerator>();
            services.AddSingleton<IAIContentGenerator>(new HuggingFaceGenerator(new HttpClient(), Environment.GetEnvironmentVariable("HUGGINGFACE_API_TOKEN") ?? throw new InvalidOperationException("HuggingFaceApiKey not found.")));
            services.AddHttpClient<ILinkedinService, LinkedInService>();
            services.AddSingleton<ILinkedinService>(provider =>
            {
                var logger = provider.GetRequiredService<ILogger<LinkedInService>>();
                return new LinkedInService(logger, Environment.GetEnvironmentVariable("LINKEDIN_USER_ID") ?? throw new InvalidOperationException("LinkedIn User ID not found."), Environment.GetEnvironmentVariable("LINKEDIN_USER_SECRET") ?? throw new InvalidOperationException("LinkedIn Secret Id not found."));
            });

            return services.BuildServiceProvider();
        }

        public static AppConfig LoadConfiguration()
        {
            string json = File.ReadAllText("appsettings.json");
            return JsonSerializer.Deserialize<AppConfig>(json) ?? throw new InvalidOperationException("json not found.");
        }

    }
    
}
