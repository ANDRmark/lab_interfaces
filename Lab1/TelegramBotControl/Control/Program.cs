using System;
using Newtonsoft;

namespace TelegramBotControl
{
    class Program
    {
        static void Main(string[] args)
        {
            string botToken = "547209759:AAE3BX47EAeydupkaTOHPadG9wixml23ChA";
            ConsoleBotControl bc = new ConsoleBotControl(botToken);
            bc.Start();
            bc.BackupBotChats();
            Console.WriteLine("Exiting..");
        }
    }
}

