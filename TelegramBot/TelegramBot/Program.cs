using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Exceptions;
using System.IO;
using System.Linq;
using Telegram.Bot.Types.Enums;
using System.Net;

namespace TelegramBotExperiments
{

    class Program
    {
        static ITelegramBotClient bot = new TelegramBotClient(System.IO.File.ReadAllText("Token.txt")); // Инициализация бота 
        /// <summary>
        /// Метод проверки обновлений
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="update"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        async static Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            string test = Newtonsoft.Json.JsonConvert.SerializeObject(update);
            Console.WriteLine(test);
            EchoText(botClient, update, cancellationToken);
            SavePhoto(botClient, update, cancellationToken);
            SaveDocument(botClient, update,cancellationToken);
            SaveAudio(botClient, update, cancellationToken);
            SaveVoice(botClient, update, cancellationToken);
        }
        /// <summary>
        /// Метод эхо отправление текста
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="update"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        async static Task EchoText(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Message!.Type == MessageType.Text)
            {
                var chatId = update.Message.Chat.Id;
                var messageText = update.Message.Text;
                Message sentMessage;
                if (messageText == "/фото")
                {
                    WebClient web = new WebClient();
                    sentMessage = await botClient.SendPhotoAsync
                        (
                        chatId: chatId,
                        photo: new Telegram.Bot.Types.InputFiles.InputOnlineFile("https://avatarko.ru/img/kartinka/33/multfilm_lyagushka_32117.jpg")
                        );
                    return;
                }
                if (messageText == "/Файлы")
                {
                    info(botClient, update, cancellationToken);
                    sentMessage = await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: "Чтобы скачать файл отправьте его название",
                    cancellationToken: cancellationToken);
                    return;
                }

                Console.WriteLine($"Возвращено '{messageText}' сообщение в чат {chatId}.");
                sentMessage = await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: "Вы сказали:\n" + messageText,
                    cancellationToken: cancellationToken);
            }
        }
        async static Task info(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            string directory = Convert.ToString(update.Message.From.FirstName);          
            string[] photo = Directory.GetFiles($@"{directory}\Photo");
            string[] audio = Directory.GetFiles($@"{directory}\Audio");
            string[] document = Directory.GetFiles($@"{directory}\Document");
            string[] voice = Directory.GetFiles($@"{directory}\Voice");
            var chatId = update.Message.Chat.Id;
            foreach (var item in photo)
            {
                string text = ($"/{item.Split('\\')[2]}");
                Message message = await botClient.SendTextMessageAsync
                (
                chatId: chatId,
                text: text
                );
            }
            foreach (var item in audio)
            {
                string text = item.Split('\\')[2];
                Message message = await botClient.SendTextMessageAsync
                (
                chatId: chatId,
                text: text
                );
            }
            foreach (var item in document)
            {
                string text = item.Split('\\')[2];
                Message message = await botClient.SendTextMessageAsync
                (
                chatId: chatId,
                text: text
                );
            }
            foreach (var item in voice)
            {
                string text = item.Split('\\')[2];
                Message message = await botClient.SendTextMessageAsync
                (
                chatId: chatId,
                text: text
                );
            }

        }
        /// <summary>
        /// Метод скачивания фото отправленных боту
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="update"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        async static Task SavePhoto(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Message!.Type == MessageType.Photo)
            {
                string directory = Convert.ToString(update.Message.From.FirstName);
                Directory.Exists(directory);
                if (System.IO.File.Exists(directory) == false)
                {
                    Directory.CreateDirectory(directory);
                    if (Directory.Exists($@"{directory}\Photo") == false)
                    {
                        Directory.CreateDirectory($@"{directory}\Photo");
                    }

                }
                var fileId = update.Message.Photo.Last().FileId;
                var fileInfo = await botClient.GetFileAsync(fileId);
                var filePath = fileInfo.FilePath;
                string destinationFilePath = $@"{directory}\Photo\{update.Message.Photo.Last().FileUniqueId}.png";
                await using FileStream fileStream = System.IO.File.OpenWrite(destinationFilePath);
                await botClient.DownloadFileAsync(filePath: filePath, destination: fileStream);
            }
        }
        /// <summary>
        /// Медот скачивание документов отрпавленных боту
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="update"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        async static Task SaveDocument(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Message!.Type == MessageType.Document)
            {
                string directory = Convert.ToString(update.Message.From.FirstName);
                Directory.Exists(directory);
                if (System.IO.File.Exists(directory) == false)
                {
                    Directory.CreateDirectory(directory);
                    if (Directory.Exists($@"{directory}\Document") == false)
                    {
                        Directory.CreateDirectory($@"{directory}\Document");
                    }

                }
                var fileId = update.Message.Document.FileId;
                var fileInfo = await botClient.GetFileAsync(fileId);
                var filePath = fileInfo.FilePath;
                string destinationFilePath = $@"{directory}\Document\{update.Message.Document.FileName}";
                await using FileStream fileStream = System.IO.File.OpenWrite(destinationFilePath);
                await botClient.DownloadFileAsync(filePath: filePath, destination: fileStream);
            }
        }
        /// <summary>
        /// Метод скачивание аудио файлов отправленных боту
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="update"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        async static Task SaveAudio(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Message!.Type == MessageType.Audio)
            {
                string directory = Convert.ToString(update.Message.From.FirstName);
                Directory.Exists(directory);
                if (System.IO.File.Exists(directory) == false)
                {
                    Directory.CreateDirectory(directory);
                    if (Directory.Exists($@"{directory}\Audio") == false)
                    {
                        Directory.CreateDirectory($@"{directory}\Audio");
                    }

                }
                var fileId = update.Message.Audio.FileId;
                var fileInfo = await botClient.GetFileAsync(fileId);
                var filePath = fileInfo.FilePath;
                string destinationFilePath = $@"{directory}\Audio\{update.Message.Audio.FileName}";
                await using FileStream fileStream = System.IO.File.OpenWrite(destinationFilePath);
                await botClient.DownloadFileAsync(filePath: filePath, destination: fileStream);
            }
        }
        /// <summary>
        /// Метод скачивания голосовых сообщений отправленных боту
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="update"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        async static Task SaveVoice(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Message!.Type == MessageType.Voice) // Проверка что отправленно боту
            {
                string directory = Convert.ToString(update.Message.From.FirstName);
                Directory.Exists(directory);
                if (System.IO.File.Exists(directory) == false)
                {
                    Directory.CreateDirectory(directory);
                    if (Directory.Exists($@"{directory}\Voice") == false)
                    {
                        Directory.CreateDirectory($@"{directory}\Voice");
                    }

                }
                var fileId = update.Message.Voice.FileId;
                var fileInfo = await botClient.GetFileAsync(fileId);
                var filePath = fileInfo.FilePath;
                string destinationFilePath = $@"{directory}\Voice\{update.Message.From.FirstName}-{update.Message.Voice.FileUniqueId}.{update.Message.Voice.MimeType.Split('/')[1]}";
                await using FileStream fileStream = System.IO.File.OpenWrite(destinationFilePath);
                await botClient.DownloadFileAsync(filePath: filePath, destination: fileStream);
            }
        }
        public static async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
         {
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(exception));
         }


        static void Main(string[] args)
        {
            Console.WriteLine("Запущен бот " + bot.GetMeAsync().Result.FirstName);
            
            var cts = new CancellationTokenSource();
            var cancellationToken = cts.Token;
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = { }, // Получать все типы обновлений
            };
            bot.StartReceiving(
                HandleUpdateAsync,
                HandleErrorAsync,
                receiverOptions,
                cancellationToken
            );
            Console.ReadLine();
        }
    }
}