namespace LinkedInAutomation.Console
{
    public class AppConfig
    {
        public required string LinkedInUSERID { get; set; }
        public required string LinkedSECRETKEY { get; set; }
        public required string HuggingFaceApiKey { get; set; }
        public required string TelegramChatId { get; set; }
        public required string TelegramBotToken { get; set; }
    }
}