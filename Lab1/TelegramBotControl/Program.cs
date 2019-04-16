using System;
using Newtonsoft;

namespace TelegramBotControl
{
    class Program
    {
        static void Main(string[] args)
        {
            string botToken = "547209759:AAE3BX47EAeydupkaTOHPadG9wixml23ChA";
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
}

