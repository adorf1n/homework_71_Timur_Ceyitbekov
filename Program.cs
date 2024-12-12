using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

class Program
{
    private static readonly TelegramBotClient Bot = new TelegramBotClient("7906217079:AAGjpNQPaD0z29Igl8yPFCd5D2xCGkZAoSY");

    static async Task Main(string[] args)
    {
        Console.WriteLine("Запуск бота...");

        var cts = new CancellationTokenSource();
        var cancellationToken = cts.Token;
        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = Array.Empty<UpdateType>() 
        };

        Bot.StartReceiving(HandleUpdateAsync, HandleErrorAsync, receiverOptions, cancellationToken);

        var botInfo = await Bot.GetMeAsync();
        Console.WriteLine($"Бот {botInfo.Username} запущен");

        Console.ReadLine();
        cts.Cancel();
    }

    private static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        if (update.Type != Telegram.Bot.Types.Enums.UpdateType.Message || update.Message!.Type != Telegram.Bot.Types.Enums.MessageType.Text)
            return;

        var message = update.Message;
        var chatId = message.Chat.Id;

        switch (message.Text)
        {
            case "/start":
                await botClient.SendTextMessageAsync(chatId,
                    "Добро пожаловать в игру 'Камень, ножницы, бумага'! \n\n" +
                    "Доступные команды:\n" +
                    "/start - запуск бота\n" +
                    "/help - правила игры\n" +
                    "/game - начать игру", cancellationToken: cancellationToken);
                break;

            case "/help":
                await botClient.SendTextMessageAsync(chatId,
                    "Правила игры:\n" +
                    "1. Камень бьёт ножницы.\n" +
                    "2. Ножницы режут бумагу.\n" +
                    "3. Бумага накрывает камень.\n" +
                    "Выберите свою фигуру, чтобы сыграть против бота!", cancellationToken: cancellationToken);
                break;

            case "/game":
                await StartGame(botClient, chatId, cancellationToken);
                break;

            case "Камень":
            case "Ножницы":
            case "Бумага":
                await HandleGameChoice(botClient, chatId, message.Text, cancellationToken);
                break;

            case "Повторить":
                await StartGame(botClient, chatId, cancellationToken);
                break;

            case "Завершить":
                await botClient.SendTextMessageAsync(chatId, "Спасибо за игру! Возвращайтесь снова!", cancellationToken: cancellationToken);
                break;

            default:
                await botClient.SendTextMessageAsync(chatId, "Неизвестная команда. Используйте /help для справки.", cancellationToken: cancellationToken);
                break;
        }
    }

    private static async Task StartGame(ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken)
    {
        var keyboard = new ReplyKeyboardMarkup(new[]
        {
            new KeyboardButton[] { "Камень", "Ножницы", "Бумага" }
        })
        {
            ResizeKeyboard = true,
            OneTimeKeyboard = true
        };

        await botClient.SendTextMessageAsync(chatId, "Сделайте свой выбор:", replyMarkup: keyboard, cancellationToken: cancellationToken);
    }

    private static async Task HandleGameChoice(ITelegramBotClient botClient, long chatId, string userChoice, CancellationToken cancellationToken)
    {
        string[] choices = { "Камень", "Ножницы", "Бумага" };
        string botChoice = choices[new Random().Next(choices.Length)];

        string result = DetermineWinner(userChoice, botChoice);

        await botClient.SendTextMessageAsync(chatId,
            $"Ваш выбор: {userChoice}\n" +
            $"Выбор бота: {botChoice}\n" +
            result, cancellationToken: cancellationToken);

        var nextStepKeyboard = new ReplyKeyboardMarkup(new[]
        {
            new KeyboardButton[] { "Повторить", "Завершить" }
        })
        {
            ResizeKeyboard = true,
            OneTimeKeyboard = true
        };

        await botClient.SendTextMessageAsync(chatId, "Хотите сыграть снова?", replyMarkup: nextStepKeyboard, cancellationToken: cancellationToken);
    }

    private static string DetermineWinner(string userChoice, string botChoice)
    {
        return (userChoice, botChoice) switch
        {
            ("Камень", "Ножницы") => "Камень ломает ножницы. Вы победили!",
            ("Камень", "Бумага") => "Камень накрыт бумагой. Вы проиграли!",
            ("Ножницы", "Бумага") => "Ножницы режут бумагу. Вы победили!",
            ("Ножницы", "Камень") => "Ножницы сломаны камнем. Вы проиграли!",
            ("Бумага", "Камень") => "Бумага накрывает камень. Вы победили!",
            ("Бумага", "Ножницы") => "Бумага разрезана ножницами. Вы проиграли!",
            (_, _) => "Ничья!"
        };
    }

    private static Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        Console.WriteLine(exception.Message);
        return Task.CompletedTask;
    }
}