using System;
using System.Collections.Generic;
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
        private FSM FSM { get; set; }
        public ConsoleBotControl(string botToken)
        {
            this.FSM = new FSM(botToken, "chatIDs.json");
        }
        public string State { get; set; }
    }

    public class FSM
    {
        
        public Dictionary<string, State> States { get; set; }
        public State CurrentState { get; set; }

        private static Dictionary<string, Func<State>> AllRegisteredStates { get; set; } = new Dictionary<string, Func<State>>();
        public static void RegisterState(string stateId, Func<State> stateCreator)
        {
            AllRegisteredStates[stateId] = stateCreator;
        }
    }

    [Flags]
    public enum ControlSignals
    {

    }
    public class State
    {
        public string Id { get; set; }
        public bool Completed { get; set; }
        public Action callback { get; set; }

        public void Execute()
        {
            callback();
        }
    }
    public class ListenMode
    {
        private CancellationTokenSource tokenSource { get; set; }
        private TelegramBot bot { get; set; }
        public string LastUser { get; set; }
        public void Listen(Action callback)
        {
            Console.WriteLine("Press any key to stop listening");
            using (this.tokenSource = new CancellationTokenSource())
            {
                var bacgroundPrinting = Task.Run(BackGroundPrinting, this.tokenSource.Token);
                Console.ReadKey();
                this.tokenSource.Cancel();
                try
                {
                    bacgroundPrinting.Wait();
                }
                catch (AggregateException e)
                {
                    if (!(e.InnerException is OperationCanceledException))
                        throw new Exception("Listen failed.", e.InnerException);
                }
            }
            callback();
        }
        public ListenMode(TelegramBot bot)
        {
            this.bot = bot;
        }

        private void BackGroundPrinting()
        {
            while (true)
            {
                this.tokenSource.Token.ThrowIfCancellationRequested();

                var updates = this.bot.GetUpdates(this.tokenSource.Token).Result;
                if (updates.Exception != null)
                {
                    throw  new Exception($"Exception occurred while getting new messages.", updates.Exception);
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

