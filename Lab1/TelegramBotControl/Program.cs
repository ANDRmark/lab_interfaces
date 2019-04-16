using System;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft;

namespace TelegramBotControl
{
    class Program
    {
        static void Main(string[] args)
        {
            string botToken = "547209759:AAE3BX47EAeydupkaTOHPadG9wixml23ChA";
            ConsoleBotControl bc = new ConsoleBotControl(botToken);
            TelegramBot TB = new TelegramBot(botToken);
            var status = TB.CheckStatus();
            Console.WriteLine($"You are controlling bot: {status.BotUsetname}");
            Updates messages = null;
            int i = 10;
            while (i-- > 0)
            {
                messages = TB.GetUpdates();
                foreach (var m in messages.IncomeMessages)
                {
                    if (m.SourceType == IncomeMessageSource.PrivateMessage)
                    {
                        Console.WriteLine($" \"{m.UserName}\" : {m.Text}");
                    }
                    else if (m.SourceType == IncomeMessageSource.GroupMessage)
                    {
                        Console.WriteLine($" \"{m.GroupTitle}\" \"{m.UserName}\" : {m.Text}");
                    }
                    else if (m.SourceType == IncomeMessageSource.ChannelPost)
                    {
                        Console.WriteLine($" \"{m.ChannelTitle}\" : {m.Text}");
                    }
                }
            }
            TB.SendMessageToUser("ANDRmark", "bot is saying hello");
            TB.SendMessageToChannel("experiment", "bot is saying hello");
            TB.SendMessageToGroup("bottestgroup", "bot is saying hello");
            TB.BackupChatIDs();
            Console.WriteLine("THE END");
            Console.ReadLine();
        }
    }
    public class ConsoleBotControl
    {
        private TelegramBot bot { get; set; }
        public ConsoleBotControl(string botToken)
        {
            this.bot = new TelegramBot(botToken, "chatIDs.json");
        }
        public string State { get; set; }
    }

    public class ListenMode
    {
        private CancellationTokenSource tokenSource { get; set; }
        private TelegramBot bot { get; set; }
        public string LastUser { get; set; }
        public void Listen(Action callback)
        {
            Console.WriteLine("Press any key to stop listening");
            tokenSource = new CancellationTokenSource();
            var bacgroundPrinting = Task.Run(new Action(BackGroundPrinting), this.tokenSource.Token);
            Console.ReadKey();
            this.tokenSource.Cancel();
            bacgroundPrinting.Wait();
            this.tokenSource.Dispose();
            callback();
        }
        public ListenMode(TelegramBot bot)
        {
            this.bot = bot;
        }

        private void BackGroundPrinting()
        {
            while (!this.tokenSource.Token.IsCancellationRequested)
            {
                var updates = this.bot.GetUpdates(this.tokenSource.Token);
                if (updates.Exception != null)
                {
                    Console.WriteLine($"Exception occurred while getting new messages. {updates.Exception.ToString()}");
                    this.tokenSource.Cancel();
                }
                else
                {
                    foreach (var m in updates.IncomeMessages)
                    {
                        if (m.SourceType == IncomeMessageSource.PrivateMessage)
                        {
                            Console.WriteLine($"(PrivateMessage) \"{m.UserName}\" : {m.Text}");
                        }
                        else if (m.SourceType == IncomeMessageSource.GroupMessage)
                        {
                            Console.WriteLine($"(GroupMessage  ) \"{m.GroupTitle}\" \"{m.UserName}\" : {m.Text}");
                        }
                        else if (m.SourceType == IncomeMessageSource.ChannelPost)
                        {
                            Console.WriteLine($"(ChannelPost   ) \"{m.ChannelTitle}\" : {m.Text}");
                        }
                    }
                }
            }
        }

    }

    public class WriteMode
    {
        public string LastUser { get; set; }
        public void Listen(Action callback)
        {
            Console.ReadKey();
            callback();
        }
    } 

    public class WriteToLastUser
    {

    }
}

