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
            Text(botClient, update, cancellationToken);
            await SavePhoto(botClient, update, cancellationToken);
            await SaveDocument(botClient, update,cancellationToken);
            await SaveAudio(botClient, update, cancellationToken);
            await SaveVoice(botClient, update, cancellationToken);
        }
        /// <summary>
        /// Метод работы с текстом и командами
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="update"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        async static Task Text(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Message!.Type == MessageType.Text)
            {
                var chatId = update.Message.Chat.Id;
                var messageText = update.Message.Text;
                Message sentMessage;
                if (messageText == "/start" ^ messageText ==  "/Start")
                {
                    sentMessage = await botClient.SendTextMessageAsync
                        (
                        chatId: chatId,
                        text: $"Добро пожаловать.\n Я умею сохранять твои фотографии, аудио, голосовые сообщения, документы" +
                        $", а ещё я очень люблю повторять за людьми" +
                        $"\nДля того чтобы я их сохранил, просто отправь их мне.\nМои команды: \n1) /Файлы - показывает все" +
                        $"сохраненные мной файлы.\n2) /Скачать \"Имя файла\" - я отправлю тебе файл, который ты сохранял. ",
                        cancellationToken: cancellationToken
                        );
                    return;
                }
                if ($"{messageText.Split(' ')[0]}" == "/Скачать") //Если первое слово в сообщение /Скачать запускает метод отправки файла пользователю.
                {
                    Upload(botClient, update, cancellationToken);
                    return;
                }
                if (messageText == "/Файлы") //Команда показывающая все сохраненные пользователем файлы
                {
                    info(botClient, update, cancellationToken);
                    return;
                }
                    Console.WriteLine($"Возвращено '{messageText}' сообщение в чат {chatId}."); //Эхо сообщения
                    sentMessage = await botClient.SendTextMessageAsync(
                        chatId: chatId,
                        text: "Вы сказали:" + messageText,
                        cancellationToken: cancellationToken);
                
            }
        }
        /// <summary>
        /// Отправка файлов пользователю
        /// </summary>
        /// <returns></returns>
        async static Task Upload(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            string directory = Convert.ToString(update.Message.From.FirstName);
            string[] file = Directory.GetFiles(directory);
            foreach (string fileItem in file)
            {
                if (update.Message.Text == $"/Скачать {fileItem.Split('\\')[1]}")
                {
                    string path = $@".\{update.Message.From.FirstName}\{update.Message.Text.Split('/', ' ')[2]}";//Путь к файлу
                    var chatId = update.Message.Chat.Id;
                    string filename = update.Message.Text.Split('/', ' ')[2]; // Имя файла
                    await using Stream stream = System.IO.File.OpenRead($"{path}");
                    Message message = await botClient.SendDocumentAsync //Отправка файла пользователю
                        (
                        chatId: chatId,
                        document: new Telegram.Bot.Types.InputFiles.InputOnlineFile(content: stream, fileName: filename),
                        cancellationToken: cancellationToken
                        );
                }
            }
            
        }
        /// <summary>
        /// Метод для получения информации о загруженных файлах
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="update"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        async static Task info(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            string directory = Convert.ToString(update.Message.From.FirstName); //Директория хранения файлов    
            string[] file = Directory.GetFiles($@"{directory}"); //Наименования файлов в директории 
            var chatId = update.Message.Chat.Id;
            foreach (var item in file) // Отправляем наименование файлов пользователю
            {
                string text = item.Split('\\')[1];
                Message message = await botClient.SendTextMessageAsync
                (
                chatId: chatId,
                text: text,
                cancellationToken: cancellationToken
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
            if (update.Message!.Type == MessageType.Photo) // Проверка, что боту отправили фото
            {
                string directory = Convert.ToString(update.Message.From.FirstName); //Директория хранения 
                if (System.IO.File.Exists(directory) == false) //Проверка существования директории
                {
                    Directory.CreateDirectory(directory);                    
                }
                var fileId = update.Message.Photo.Last().FileId;
                var fileInfo = await botClient.GetFileAsync(fileId); 
                var filePath = fileInfo.FilePath;
                string destinationFilePath = $@"{directory}\{update.Message.Photo.Last().FileUniqueId}.png"; //Путь для сохранения фото
                await using FileStream fileStream = System.IO.File.OpenWrite(destinationFilePath); 
                await botClient.DownloadFileAsync(filePath: filePath, destination: fileStream); //Скачивание файла 
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
            if (update.Message!.Type == MessageType.Document) //Проверка, что боту отправили документ
            {
                string directory = Convert.ToString(update.Message.From.FirstName); //Путь к директории 
                if (System.IO.File.Exists(directory) == false) //Проверка наличия директории 
                {
                    Directory.CreateDirectory(directory);
                }
                var fileId = update.Message.Document.FileId;
                var fileInfo = await botClient.GetFileAsync(fileId); //Преобразование файоа в объект
                var filePath = fileInfo.FilePath;
                string destinationFilePath = $@"{directory}\{update.Message.Document.FileName}";
                await using FileStream fileStream = System.IO.File.OpenWrite(destinationFilePath);
                await botClient.DownloadFileAsync(filePath: filePath, destination: fileStream); //Асинхронно скачиваем файл
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
            if (update.Message!.Type == MessageType.Audio)//Проверка, что боту было отправлено аудио
            {
                string directory = Convert.ToString(update.Message.From.FirstName);//Путь к директории
                if (System.IO.File.Exists(directory) == false) //Проверка наличия директории
                {
                    Directory.CreateDirectory(directory);                    
                }
                var fileId = update.Message.Audio.FileId;
                var fileInfo = await botClient.GetFileAsync(fileId);//Преобразование аудио в объект
                var filePath = fileInfo.FilePath;
                string destinationFilePath = $@"{directory}\{update.Message.Audio.FileName}";
                await using FileStream fileStream = System.IO.File.OpenWrite(destinationFilePath);
                await botClient.DownloadFileAsync(filePath: filePath, destination: fileStream);//Асинхронно скачиваем файл
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
            if (update.Message!.Type == MessageType.Voice) // Проверка, что боту отправленно голосовое сообщение
            {
                string directory = Convert.ToString(update.Message.From.FirstName);//Путь к директории хранения 
                if (System.IO.File.Exists(directory) == false) //Проверка наличия директории
                {
                    Directory.CreateDirectory(directory);                    
                }
                var fileId = update.Message.Voice.FileId;
                var fileInfo = await botClient.GetFileAsync(fileId); //Преобразование голосового сообщения в объект
                var filePath = fileInfo.FilePath;
                string destinationFilePath = $@"{directory}\{update.Message.From.FirstName}-
                                                {update.Message.Voice.FileUniqueId}.{update.Message.Voice.MimeType.Split('/')[1]}";
                await using FileStream fileStream = System.IO.File.OpenWrite(destinationFilePath);//Асинхронно скачиваем файл
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