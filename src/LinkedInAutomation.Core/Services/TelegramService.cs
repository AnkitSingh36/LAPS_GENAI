using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LinkedInAutomation.Core.Models;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace LinkedInAutomation.Core.Services
{
    public class TelegramService : ITelegramService
    {
        private readonly ITelegramBotClient _botClient;
        private readonly long _authorizedChatId;
        private readonly ILogger<TelegramService> _logger;
        private readonly Dictionary<long, TaskCompletionSource<TelegramResponse>> _pendingResponses;

        public TelegramService(
            string botToken,
            long authorizedChatId,
            ILogger<TelegramService> logger)
        {
            _botClient = new TelegramBotClient(botToken);
            _authorizedChatId = authorizedChatId;
            _logger = logger;
            _pendingResponses = new Dictionary<long, TaskCompletionSource<TelegramResponse>>();

            
            _botClient.StartReceiving(
                HandleUpdateAsync,
                HandleErrorAsync
            );
        }

        public async Task<string> AskForTopic()
        {
            

            await _botClient.SendTextMessageAsync(
                _authorizedChatId,
                "What topic would you like to post about today?"
            );

            var response = await WaitForResponse(TimeSpan.FromMinutes(30));
            return response.ResponseText!;
        }

        public async Task<bool> RequestApproval(string content)
        {
            var keyboard = new InlineKeyboardMarkup(new[]
            {
                new []
                {
                    InlineKeyboardButton.WithCallbackData("✅ Approve", "approve"),
                    InlineKeyboardButton.WithCallbackData("❌ Reject", "reject")
                }
            });

            await _botClient.SendTextMessageAsync(
                _authorizedChatId,
                $"Generated LinkedIn Post:\n\n{content}\n\nWould you like to post this?",
                replyMarkup: keyboard
            );

            var response = await WaitForResponse(TimeSpan.FromHours(1));
            return response.IsApproved;
        }

        public async Task SendMessage(string message)
        {
            await _botClient.SendTextMessageAsync(_authorizedChatId, message);
        }

        public async Task SendErrorNotification(string error)
        {
            await _botClient.SendTextMessageAsync(
                _authorizedChatId,
                $"❌ Error occurred:\n{error}",
                parseMode: ParseMode.Html
            );
        }

        public bool IsUserAuthorized(long chatId)
        {
            return chatId ==  _authorizedChatId;
        }

        public async Task<TelegramResponse> WaitForResponse(TimeSpan? timeout = null)
        {
            timeout ??= TimeSpan.FromMinutes(30);
            var tcs = new TaskCompletionSource<TelegramResponse>();
            _pendingResponses[_authorizedChatId] = tcs;

            using var cts = new CancellationTokenSource(timeout.Value);
            cts.Token.Register(() => tcs.TrySetCanceled(), useSynchronizationContext: false);

            try
            {
                return await tcs.Task;
            }
            catch (OperationCanceledException)
            {
                throw new TimeoutException("No response received within the specified timeout.");
            }
            finally
            {
                _pendingResponses.Remove(_authorizedChatId);
            }
        }

        private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            try
            {
                if (update.Message?.Text != null)
                {
                    var chatId = update.Message.Chat.Id;
                    if (!IsUserAuthorized(chatId))
                    {
                        await SendMessage("Unauthorized access.");
                        return;
                    }

                    if (_pendingResponses.TryGetValue(chatId, out var tcs))
                    {
                        var response = new TelegramResponse
                        {
                            MessageId = update.Message.MessageId,
                            ResponseText = update.Message.Text,
                            ReceivedAt = DateTime.UtcNow,
                            ChatId = chatId,
                            Username = update.Message.From?.Username,
                            IsApproved = update.Message.Text.ToLower() == "yes"
                        };

                        tcs.TrySetResult(response);
                    }
                }
                else if (update.CallbackQuery != null)
                {
                    var chatId = update.CallbackQuery.Message!.Chat.Id;
                    if (_pendingResponses.TryGetValue(chatId, out var tcs))
                    {
                        var response = new TelegramResponse
                        {
                            MessageId = update.CallbackQuery.Message.MessageId,
                            ResponseText = update.CallbackQuery.Data,
                            ReceivedAt = DateTime.UtcNow,
                            ChatId = chatId,
                            Username = update.CallbackQuery.From.Username,
                            IsApproved = update.CallbackQuery.Data == "approve"
                        };

                        tcs.TrySetResult(response);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling telegram update");
            }
        }

        private Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            _logger.LogError(exception, "Error in Telegram bot");
            return Task.CompletedTask;
        }
    }
}
