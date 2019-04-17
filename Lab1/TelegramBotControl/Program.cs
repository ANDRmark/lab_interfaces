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
            bc.Start();
            Console.WriteLine("Exiting..");
            //TelegramBot TB = new TelegramBot(botToken);
            //var status = TB.CheckStatus();
            //Console.WriteLine($"You are controlling bot: {status.BotUsetname}");
            //Updates messages = null;
            //int i = 10;
            //while (i-- > 0)
            //{
            //    messages = TB.GetUpdates();
            //    foreach (var m in messages.IncomeMessages)
            //    {
            //        if (m.SourceType == IncomeMessageSource.PrivateMessage)
            //        {
            //            Console.WriteLine($" \"{m.UserName}\" : {m.Text}");
            //        }
            //        else if (m.SourceType == IncomeMessageSource.GroupMessage)
            //        {
            //            Console.WriteLine($" \"{m.GroupTitle}\" \"{m.UserName}\" : {m.Text}");
            //        }
            //        else if (m.SourceType == IncomeMessageSource.ChannelPost)
            //        {
            //            Console.WriteLine($" \"{m.ChannelTitle}\" : {m.Text}");
            //        }
            //    }
            //}
            //TB.SendMessageToUser("ANDRmark", "bot is saying hello");
            //TB.SendMessageToChannel("experiment", "bot is saying hello");
            //TB.SendMessageToGroup("bottestgroup", "bot is saying hello");
            //TB.BackupChatIDs();
            //Console.WriteLine("THE END");
            //Console.ReadLine();
        }
    }
    public class ConsoleBotControl
    {
        private FSM FSM { get; set; }
        public ConsoleBotControl(string botToken)
        {
            this.FSM = new FSM(botToken);
        }

        public void Start()
        {
            this.FSM.Reset();
            this.FSM.Execute();
        }
    }

    public class FSM
    {
        string startStateId { get { return MainMenuState.StateName; } }
        string endStateId { get { return HLTState.StateName; } }
        Dictionary<string, State> States { get; set; }
        State CurrentState { get; set; }
        TransitionTable transitionTable { get; set; }
        StateContext context { get; set; }

        private static Dictionary<string, Func<State>> AllRegisteredStates { get; set; } = new Dictionary<string, Func<State>>();
        private static void RegisterState(string stateId, Func<State> stateCreator)
        {
            FSM.AllRegisteredStates[stateId] = stateCreator;
        }
        static void RegisterAllStates()
        {
            FSM.RegisterState(MainMenuState.StateName, () => new MainMenuState());
            FSM.RegisterState(ListenState.StateName, () => new ListenState());
            FSM.RegisterState(HLTState.StateName, () => new HLTState());
        }
        static FSM()
        {
            RegisterAllStates();
        }

        public FSM(string botToken)
        {
            this.States = new Dictionary<string, State>();
            foreach(var stateCreator in FSM.AllRegisteredStates.Values)
            {
                State s = stateCreator();
                this.States[s.Id] = s;
            }
            this.transitionTable = new TransitionTable();
            this.transitionTable.AddTransition(HLTState.StateName, MainMenuState.StateName, s => true);
            this.transitionTable.AddTransition(MainMenuState.StateName, ListenState.StateName, s => s == Signals.ListenToMessages);
            this.transitionTable.AddTransition(MainMenuState.StateName, HLTState.StateName, s => s == Signals.Exit);
            this.transitionTable.AddTransition(ListenState.StateName, MainMenuState.StateName, s => (~s & Signals.AnswerLastChat) > 0);
            this.Reset();

            this.context = new StateContext();
            this.context.Bot = new TelegramBot(botToken, "chatIDs.json");
            this.context.Signals = Signals.None;
        }

        public void Reset()
        {
            this.CurrentState = this.States[startStateId];
        }

        public void Execute()
        {
            while (this.CurrentState.Id != this.endStateId)
            {
                this.CurrentState.Execute(context);
                this.TransitToNextState();
            }
        }
        private void TransitToNextState()
        {
            string nextStateId = this.transitionTable.GetNextState(this.CurrentState.Id, this.context.Signals);
            this.CurrentState = this.States[nextStateId];
        }
    }

    public class StateContext
    {
        public Signals Signals { get; set; }
        public TelegramBot Bot { get; set; }
        public IncomeMessage LastMessage { get; set; }
    }

    public class TransitionTable
    {
        Dictionary<string, Dictionary<string, Func<Signals,bool>>> table { get; set; }
        public TransitionTable()
        {
            this.table = new Dictionary<string, Dictionary<string, Func<Signals, bool>>>();
        }
        public void AddTransition(string currentState, string nextState, Func<Signals, bool> condition)
        {
            if (!this.table.ContainsKey(currentState))
            {
                this.table[currentState] = new Dictionary<string, Func<Signals, bool>>();
            }
            this.table[currentState][nextState] = condition;
        }
        public string GetNextState(string currentState, Signals currentSignals)
        {
            if (this.table.ContainsKey(currentState))
            {
                var transitionsForCurrentState = this.table[currentState];
                if(transitionsForCurrentState != null)
                {
                    foreach(var possibleState in transitionsForCurrentState.Keys)
                    {
                        if (transitionsForCurrentState[possibleState](currentSignals))
                        {
                            return possibleState;
                        }
                    }
                }
            }
            throw new Exception($"Transition for {currentState} not defined when signals {currentSignals}.");
        }
    }

    [Flags]
    public enum Signals
    {
        None = 0,
        ListenToMessages = 1 << 0,
        WriteNewMessage = 1 << 1,
        AnswerLastChat = 1 << 3,
        Exit  = 1 << 4,
    }
    public class State
    {
        public virtual string Id { get; set; }
        public virtual bool Completed { get; set; }
        public virtual Action callback { get; set; }

        public virtual void Execute(StateContext context)
        {
            callback?.Invoke();
        }

    }

    public class HLTState : State
    {
        public static readonly string StateName = "HLTState";
        public HLTState(string id = null)
        {
            this.Id = id ?? HLTState.StateName;
        }
        public override void Execute(StateContext context)
        {
            context.Signals = Signals.None;
            base.Execute(context);
        }
    }
    public class WriteNewMessageState : State
    {
        public static readonly string StateName = "WriteNewMessageState";
        public WriteNewMessageState(string id = null)
        {
            this.Id = id ?? MainMenuState.StateName;
        }
        public override void Execute(StateContext context)
        {
            Console.Clear();
            Console.WriteLine("Send Message menu");
            Console.WriteLine("Choose one option :");
            Console.WriteLine("1 : Send message to user");
            Console.WriteLine("2 : Send message to group");
            Console.WriteLine("3 : Send message to channel");
            var key = Console.ReadKey();

            if (key.KeyChar == '1') context.Signals = Signals.WriteNewMessage;
            else
            if (key.KeyChar == '2') context.Signals = Signals.WriteNewMessage;
            else
            if (key.KeyChar == '3') context.Signals = Signals.AnswerLastChat;
            else
                context.Signals = Signals.Exit;
            base.Execute(context);
        }
    }

    public class MainMenuState: State
    {
        public static readonly string StateName = "MainMenuState";
        public MainMenuState(string id = null)
        {
            this.Id = id ?? MainMenuState.StateName;
        }
        public override void Execute(StateContext context)
        {
            Console.Clear();
            Console.WriteLine("Main menu");
            Console.WriteLine("Choose one option :");
            Console.WriteLine("1 : Listen to messages");
            Console.WriteLine("2 : Write new message");
            Console.WriteLine("3 : Answer to last chat");
            var key = Console.ReadKey();

            if (key.KeyChar == '1') context.Signals = Signals.ListenToMessages;
            else
            if (key.KeyChar == '2') context.Signals = Signals.WriteNewMessage;
            else
            if (key.KeyChar == '3') context.Signals = Signals.AnswerLastChat;
            else
                context.Signals = Signals.Exit;
            base.Execute(context);
        }
    }
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
            this.Listen();
            base.Execute(context);
        }

        private void Listen()
        {
            Console.Clear();
            Console.WriteLine("Listening to messages.");
            Console.WriteLine("Press 'a' to answer last message. Press any key to stop listening.");
            using (this.tokenSource = new CancellationTokenSource())
            {
                var bacgroundPrinting = Task.Run((Action)BackGroundPrinting, this.tokenSource.Token);
                var key = Console.ReadKey();

                if (key.KeyChar == 'a' && this.lastMessage != null)
                {
                    this.context.Signals = Signals.AnswerLastChat;
                    this.context.LastMessage = this.lastMessage;
                }
                else
                {
                    this.context.Signals = Signals.None;
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

