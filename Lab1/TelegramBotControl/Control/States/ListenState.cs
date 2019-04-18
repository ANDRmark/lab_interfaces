using System;
using System.Threading;
using System.Threading.Tasks;

namespace TelegramBotControl
{
    public class ListenState : State
    {
        public static readonly string StateName = "ListenState";
        private CancellationTokenSource tokenSource { get; set; }
        private StateContext context { get; set; }
        private IncomeMessage lastMessage { get; set; }

        public ListenState(string id = null)
        {
            this.Id = id ?? ListenState.StateName;
        }

        public override void Execute(StateContext context)
        {
            this.context = context;
            this.Listen(context);
            base.Execute(context);
        }

        private void Listen(StateContext context)
        {
            Console.WriteLine("Listening to messages.");
            Console.WriteLine("Press 'a' to answer last message. Press any key to stop listening.");
            using (this.tokenSource = new CancellationTokenSource())
            {
                var bacgroundPrinting = Task.Run((Action)BackGroundPrinting, this.tokenSource.Token);
                var key = Console.ReadKey();

                this.context.LastMessage = this.lastMessage;
                if (key.KeyChar == 'a')
                {
                    Console.WriteLine();
                    this.context.Signals = Signals.ResponseLastChatFromListening;
                }
                else
                {
                    this.context.Signals = Signals.Exit;
                }

                this.tokenSource.Cancel();
                try
                {
                    bacgroundPrinting.Wait();
                }
                catch (AggregateException e)
                {
                    if (!bacgroundPrinting.IsCanceled)
                        throw new Exception("Listen failed.", e.InnerException);
                }
            }
        }

        private void BackGroundPrinting()
        {
            while (true)
            {
                this.tokenSource.Token.ThrowIfCancellationRequested();

                Updates updates = null;
                var updatesTask = this.context.Bot.GetUpdates(this.tokenSource.Token);
                try
                {
                    updates = updatesTask.Result;
                }
                catch (AggregateException e)
                {
                    if (updatesTask.IsCanceled) this.tokenSource.Token.ThrowIfCancellationRequested();
                    throw;
                }
                if (updates.Exception != null)
                {
                    Console.WriteLine($"Exception occurred while getting new messages. {updates.Exception}");
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
                        this.lastMessage = m;
                    }
                }
            }
        }

    }
}

